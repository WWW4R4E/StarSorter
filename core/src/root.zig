const std = @import("std");
const http = std.http;

pub const Client = @import("github/Client.zig").GitHubClient;
pub const ClientError = @import("github/Client.zig").Error;
pub const types = @import("github/types.zig");

const rest_core = @import("github/rest/core.zig");

// ─── C ABI Types ───

pub const GhError = enum(c_int) {
    success = 0,
    network_error = -1,
    unauthorized = -2,
    not_found = -3,
    rate_limited = -4,
    server_error = -5,
    parse_error = -6,
    forbidden = -7,
    http_error = -8,
    unknown = -99,
};

pub const GhResult = extern struct {
    data: ?[*:0]u8,
    error_code: c_int,
};

pub const ClientWrapper = struct {
    client: Client,
    threaded: std.Io.Threaded,
};

// ─── Error Helpers ───

fn ghErrorFromCode(err: anyerror) c_int {
    return switch (err) {
        error.NetworkError => @intFromEnum(GhError.network_error),
        error.Unauthorized => @intFromEnum(GhError.unauthorized),
        error.NotFound => @intFromEnum(GhError.not_found),
        error.RateLimitExceeded => @intFromEnum(GhError.rate_limited),
        error.ServerError => @intFromEnum(GhError.server_error),
        error.JsonParseFailed => @intFromEnum(GhError.parse_error),
        else => @intFromEnum(GhError.unknown),
    };
}

fn ghErrorFromStatus(status: http.Status) c_int {
    return switch (status.class()) {
        .client_error => if (status == .forbidden) @intFromEnum(GhError.forbidden) else @intFromEnum(GhError.http_error),
        .server_error => @intFromEnum(GhError.server_error),
        else => @intFromEnum(GhError.http_error),
    };
}

pub fn friendlyError(err: ClientError) []const u8 {
    return switch (err) {
        error.NetworkError => "网络连接失败，请检查网络/代理设置",
        error.Unauthorized => "认证失败，请检查 GITHUB_TOKEN 是否有效",
        error.NotFound => "请求的资源不存在 (404)",
        error.RateLimitExceeded => "API 速率限制已耗尽，请稍后再试",
        error.ServerError => "GitHub 服务器返回了服务端错误 (5xx)",
        error.HttpError => "请求返回了非预期的 HTTP 状态码",
        error.JsonParseFailed => "解析 GitHub API 返回的数据失败",
        error.OutOfMemory => "内存不足",
        error.Canceled => "请求被取消",
        error.UnsupportedUriScheme => "不支持的 URI 协议",
        error.UriMissingHost => "URI 缺少主机名",
        error.CertificateBundleLoadFailure => "无法加载 TLS 根证书，请检查系统证书",
        error.TlsInitializationFailed => "TLS 初始化失败，可能是代理/cert 问题",
        error.ConnectionRefused => "连接被拒绝",
        error.ConnectionResetByPeer => "连接被对端重置",
        error.Unexpected => "网络异常 (可能是代理未配置或 TLS 握手失败)",
        else => "未知错误",
    };
}

fn ghResultFromError(err: ClientError, allocator: std.mem.Allocator) GhResult {
    const msg = friendlyError(err);
    const ptr = allocator.dupeZ(u8, msg) catch |alloc_err| return .{ .data = null, .error_code = ghErrorFromCode(alloc_err) };
    return .{ .data = ptr.ptr, .error_code = ghErrorFromCode(err) };
}

// ─── Serialization Helpers ───

fn allocOwned(bytes: []const u8, allocator: std.mem.Allocator) GhResult {
    const ptr = allocator.dupeZ(u8, bytes) catch |err| return .{
        .data = null,
        .error_code = ghErrorFromCode(err),
    };
    return .{ .data = ptr.ptr, .error_code = 0 };
}

fn requestBytesResult(ctx: rest_core.ApiContext, method: http.Method, path: []const u8, query: ?[]const u8, request_body: ?[]const u8) GhResult {
    const raw = rest_core.requestBytes(ctx, method, path, query, request_body) catch |err| {
        return .{ .data = null, .error_code = ghErrorFromCode(err) };
    };
    defer ctx.allocator.free(raw.body);

    if (raw.status.class() != .success) {
        // Non-success status with body available (e.g. 403 forbidden)
        return allocOwnedWithCode(raw.body, ctx.allocator, ghErrorFromStatus(raw.status));
    }

    return allocOwned(raw.body, ctx.allocator);
}

fn allocOwnedWithCode(bytes: []const u8, allocator: std.mem.Allocator, error_code: c_int) GhResult {
    const ptr = allocator.dupeZ(u8, bytes) catch |err| return .{
        .data = null,
        .error_code = ghErrorFromCode(err),
    };
    return .{ .data = ptr.ptr, .error_code = error_code };
}

fn jsonStringify(value: anytype, allocator: std.mem.Allocator) GhResult {
    const bytes = std.json.Stringify.valueAlloc(allocator, value, .{}) catch |err| return .{
        .data = null,
        .error_code = ghErrorFromCode(err),
    };
    defer allocator.free(bytes);
    const owned = allocator.dupeZ(u8, bytes) catch |err| return .{
        .data = null,
        .error_code = ghErrorFromCode(err),
    };
    return .{ .data = owned.ptr, .error_code = 0 };
}

// ─── Lifecycle ───

pub export fn gh_init(token: [*:0]const u8, proxy_url: ?[*:0]const u8) ?*ClientWrapper {
    const allocator = std.heap.page_allocator;
    const wrapper = allocator.create(ClientWrapper) catch return null;
    wrapper.*.threaded = std.Io.Threaded.init(allocator, .{});
    errdefer wrapper.threaded.deinit();

    const io = wrapper.threaded.io();
    const token_slice = std.mem.sliceTo(token, 0);
    wrapper.client = Client.init(allocator, io, token_slice);

    wrapper.client.initProxies(if (proxy_url) |url| std.mem.sliceTo(url, 0) else null) catch {
        allocator.destroy(wrapper);
        return null;
    };
    return wrapper;
}

pub export fn gh_deinit(wrapper: ?*ClientWrapper) void {
    if (wrapper) |w| {
        const allocator = std.heap.page_allocator;
        w.client.deinit();
        w.threaded.deinit();
        allocator.destroy(w);
    }
}

pub export fn gh_free_string(s: ?[*:0]u8) void {
    if (s) |ptr| {
        const allocator = std.heap.page_allocator;
        const len = std.mem.len(ptr);
        allocator.free(ptr[0..len :0]);
    }
}

// ─────────── User API ───────────

pub export fn gh_get_current_user(wrapper: *ClientWrapper) GhResult {
    wrapper.client.rateLimitDelay();
    return requestBytesResult(wrapper.client.apiCtx(), .GET, "/user", null, null);
}

pub export fn gh_get_starred_repos(wrapper: *ClientWrapper, page: u32, per_page: u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const query = std.fmt.allocPrint(ctx.allocator, "page={d}&per_page={d}&sort=created&direction=desc", .{ page, per_page }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(query);
    return requestBytesResult(ctx, .GET, "/user/starred", query, null);
}

pub export fn gh_get_all_starred_repos(wrapper: *ClientWrapper) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();

    // Build the JSON array by merging raw GitHub JSON from each page.
    // This avoids parsing into structs and re-serializing, which can cause
    // memory corruption when string data spans multiple arena allocations.
    var all = std.ArrayList(u8).empty;
    all.append(ctx.allocator, '[') catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };

    var page: u32 = 1;
    const per_page: u8 = 100;
    var first = true;

    while (true) {
        const query = std.fmt.allocPrint(ctx.allocator, "page={d}&per_page={d}&sort=created&direction=desc", .{ page, per_page }) catch return .{
            .data = null,
            .error_code = @intFromEnum(GhError.unknown),
        };
        defer ctx.allocator.free(query);

        const raw = rest_core.requestBytes(ctx, .GET, "/user/starred", query, null) catch |err| return ghResultFromError(err, ctx.allocator);
        defer ctx.allocator.free(raw.body);

        // Empty array "[]" means no more results
        if (raw.body.len <= 2) break;

        if (!first) {
            all.append(ctx.allocator, ',') catch return .{
                .data = null,
                .error_code = @intFromEnum(GhError.unknown),
            };
        }
        first = false;

        // Strip the outer [ ] and append the inner content
        const inner = raw.body[1 .. raw.body.len - 1];
        all.appendSlice(ctx.allocator, inner) catch return .{
            .data = null,
            .error_code = @intFromEnum(GhError.unknown),
        };

        page += 1;
    }

    all.append(ctx.allocator, ']') catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };

    return allocOwned(all.items, ctx.allocator);
}

pub export fn gh_unstar_repository(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    wrapper.client.unstarRepository(std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0)) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return .{ .data = null, .error_code = 0 };
}

pub export fn gh_star_repository(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    wrapper.client.starRepository(std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0)) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return .{ .data = null, .error_code = 0 };
}

pub export fn gh_get_user_forks(wrapper: *ClientWrapper) GhResult {
    wrapper.client.rateLimitDelay();
    return requestBytesResult(wrapper.client.apiCtx(), .GET, "/user/repos", "type=forks&sort=updated&per_page=100", null);
}

// ─────────── Repository API ───────────

pub export fn gh_get_repository(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .GET, path, null, null);
}

pub export fn gh_get_repository_readme(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/readme", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .GET, path, null, null);
}

pub export fn gh_get_repo_languages(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/languages", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .GET, path, null, null);
}

pub export fn gh_get_latest_release(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/releases/latest", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .GET, path, null, null);
}

pub export fn gh_get_contributors(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/contributors", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .GET, path, null, null);
}

pub export fn gh_get_forks(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8, sort: [*:0]const u8, per_page: u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/forks", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    const query = std.fmt.allocPrint(ctx.allocator, "sort={s}&per_page={d}", .{ std.mem.sliceTo(sort, 0), per_page }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(query);
    return requestBytesResult(ctx, .GET, path, query, null);
}

pub export fn gh_get_commits(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8, per_page: u8, page: u32) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/commits", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    const query = std.fmt.allocPrint(ctx.allocator, "per_page={d}&page={d}", .{ per_page, page }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(query);
    return requestBytesResult(ctx, .GET, path, query, null);
}

pub export fn gh_get_tags(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/tags", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .GET, path, null, null);
}

pub export fn gh_check_starred(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/user/starred/{s}/{s}", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    const raw = rest_core.requestBytes(ctx, .GET, path, null, null) catch |err| {
        if (err == error.NotFound) {
            return allocOwned("false", ctx.allocator);
        }
        return .{ .data = null, .error_code = ghErrorFromCode(err) };
    };
    ctx.allocator.free(raw.body);
    return allocOwned("true", ctx.allocator);
}

pub export fn gh_get_organization(wrapper: *ClientWrapper, org: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/orgs/{s}", .{std.mem.sliceTo(org, 0)}) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .GET, path, null, null);
}

// ─────────── Release API ───────────

pub export fn gh_get_releases(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8, page: u32, per_page: u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/releases", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    const query = std.fmt.allocPrint(ctx.allocator, "page={d}&per_page={d}", .{ page, per_page }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(query);
    return requestBytesResult(ctx, .GET, path, query, null);
}

pub export fn gh_fetch_all_releases(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    const releases = wrapper.client.fetchAllReleasesForRepo(std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0)) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return jsonStringify(releases, wrapper.client.allocator);
}

pub export fn gh_get_incremental_releases(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8, since: i64) GhResult {
    const releases = wrapper.client.getIncrementalRepositoryReleases(std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0), since) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return jsonStringify(releases, wrapper.client.allocator);
}

// ─────────── Search API ───────────

pub export fn gh_search_most_stars(wrapper: *ClientWrapper, page: u32, per_page: u8) GhResult {
    const results = wrapper.client.searchMostStars(page, per_page) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return jsonStringify(results, wrapper.client.allocator);
}

pub export fn gh_search_most_forks(wrapper: *ClientWrapper, page: u32, per_page: u8) GhResult {
    const results = wrapper.client.searchMostForks(page, per_page) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return jsonStringify(results, wrapper.client.allocator);
}

pub export fn gh_search_repositories(
    wrapper: *ClientWrapper,
    query: [*:0]const u8,
    platform: c_int,
    language: c_int,
    sort: c_int,
    order: c_int,
    page: u32,
    per_page: u8,
) GhResult {
    const results = wrapper.client.searchRepositories(
        std.mem.sliceTo(query, 0),
        @enumFromInt(@as(i32, @intCast(platform))),
        @enumFromInt(@as(i32, @intCast(language))),
        @enumFromInt(@as(i32, @intCast(sort))),
        @enumFromInt(@as(i32, @intCast(order))),
        page,
        per_page,
    ) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return jsonStringify(results, wrapper.client.allocator);
}

pub export fn gh_search_daily_devs(wrapper: *ClientWrapper, page: u32, per_page: u8) GhResult {
    const users = wrapper.client.searchDailyDevs(page, per_page) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return jsonStringify(users, wrapper.client.allocator);
}

pub export fn gh_search_by_topic(wrapper: *ClientWrapper, topic: [*:0]const u8, page: u32, per_page: u8) GhResult {
    const topics = [_][]const u8{std.mem.sliceTo(topic, 0)};
    const results = wrapper.client.searchByTopic(&topics, page, per_page) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return jsonStringify(results, wrapper.client.allocator);
}

pub export fn gh_get_topic_repos(wrapper: *ClientWrapper, page: u32, per_page: u8) GhResult {
    const results = wrapper.client.getTopicRepositories(page, per_page) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return jsonStringify(results, wrapper.client.allocator);
}

pub export fn gh_get_hot_release_repos(wrapper: *ClientWrapper, page: u32, per_page: u8) GhResult {
    const results = wrapper.client.getHotReleaseRepositories(page, per_page) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return jsonStringify(results, wrapper.client.allocator);
}

pub export fn gh_get_most_popular(wrapper: *ClientWrapper, page: u32, per_page: u8) GhResult {
    const results = wrapper.client.getMostPopular(page, per_page) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return jsonStringify(results, wrapper.client.allocator);
}

pub export fn gh_search_trending(wrapper: *ClientWrapper, language: c_int, range: c_int) GhResult {
    const results = wrapper.client.searchTrending(
        @enumFromInt(@as(i32, @intCast(language))),
        @enumFromInt(@as(i32, @intCast(range))),
    ) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return jsonStringify(results, wrapper.client.allocator);
}

// ─────────── Fork API ───────────

pub export fn gh_sync_fork(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/merge-upstream", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    const body = std.fmt.allocPrint(ctx.allocator, "{{\"branch\":\"main\"}}", .{}) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(body);
    return requestBytesResult(ctx, .POST, path, null, body);
}

pub export fn gh_check_fork_sync(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8, branch: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/compare/{s}", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0), std.mem.sliceTo(branch, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .GET, path, null, null);
}

pub export fn gh_get_branches(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/branches", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .GET, path, null, null);
}

// ─────────── Actions API ───────────

pub export fn gh_get_workflows(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const workflows = wrapper.client.getRepositoryWorkflows(std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0)) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return jsonStringify(workflows, wrapper.client.allocator);
}

pub export fn gh_trigger_workflow(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8, workflow_path: [*:0]const u8, ref: [*:0]const u8) GhResult {
    wrapper.client.triggerWorkflowRun(std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0), std.mem.sliceTo(workflow_path, 0), std.mem.sliceTo(ref, 0)) catch |err| return ghResultFromError(err, wrapper.client.allocator);
    return .{ .data = null, .error_code = 0 };
}

// ─────────── Notifications API ───────────

fn buildNotificationQuery(allocator: std.mem.Allocator, all: bool, participating: bool, since: ?[*:0]const u8, before: ?[*:0]const u8, page: u32, per_page: u8) ![]const u8 {
    var buf = std.ArrayList(u8).empty;
    var num_buf: [32]u8 = undefined;
    try buf.appendSlice(allocator, "page=");
    try buf.appendSlice(allocator, try std.fmt.bufPrint(&num_buf, "{d}", .{page}));
    try buf.appendSlice(allocator, "&per_page=");
    try buf.appendSlice(allocator, try std.fmt.bufPrint(&num_buf, "{d}", .{per_page}));
    if (all) try buf.appendSlice(allocator, "&all=true");
    if (participating) try buf.appendSlice(allocator, "&participating=true");
    if (since) |s| {
        try buf.appendSlice(allocator, "&since=");
        try buf.appendSlice(allocator, std.mem.sliceTo(s, 0));
    }
    if (before) |b| {
        try buf.appendSlice(allocator, "&before=");
        try buf.appendSlice(allocator, std.mem.sliceTo(b, 0));
    }
    return buf.items;
}

pub export fn gh_get_notifications(wrapper: *ClientWrapper, all: c_int, participating: c_int, since: ?[*:0]const u8, before: ?[*:0]const u8, page: u32, per_page: u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const query = buildNotificationQuery(ctx.allocator, all != 0, participating != 0, since, before, page, per_page) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(query);
    return requestBytesResult(ctx, .GET, "/notifications", query, null);
}

pub export fn gh_mark_notifications_read(wrapper: *ClientWrapper, last_read_at: ?[*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    var body_owned: ?[]u8 = null;
    if (last_read_at) |t| {
        body_owned = std.fmt.allocPrint(ctx.allocator, "{{\"last_read_at\":\"{s}\"}}", .{std.mem.sliceTo(t, 0)}) catch return .{
            .data = null,
            .error_code = @intFromEnum(GhError.unknown),
        };
    }
    defer if (body_owned) |b| ctx.allocator.free(b);
    const body: ?[]const u8 = if (body_owned) |b| b else null;
    return requestBytesResult(ctx, .PUT, "/notifications", null, body);
}

pub export fn gh_get_thread(wrapper: *ClientWrapper, thread_id: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/notifications/threads/{s}", .{std.mem.sliceTo(thread_id, 0)}) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .GET, path, null, null);
}

pub export fn gh_mark_thread_read(wrapper: *ClientWrapper, thread_id: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/notifications/threads/{s}", .{std.mem.sliceTo(thread_id, 0)}) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .PATCH, path, null, null);
}

pub export fn gh_mark_thread_done(wrapper: *ClientWrapper, thread_id: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/notifications/threads/{s}", .{std.mem.sliceTo(thread_id, 0)}) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .DELETE, path, null, null);
}

pub export fn gh_get_thread_subscription(wrapper: *ClientWrapper, thread_id: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/notifications/threads/{s}/subscription", .{std.mem.sliceTo(thread_id, 0)}) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .GET, path, null, null);
}

pub export fn gh_set_thread_subscription(wrapper: *ClientWrapper, thread_id: [*:0]const u8, ignored: c_int) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/notifications/threads/{s}/subscription", .{std.mem.sliceTo(thread_id, 0)}) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    const body: []const u8 = if (ignored != 0) "{\"ignored\":true}" else "{\"ignored\":false}";
    return requestBytesResult(ctx, .PUT, path, null, body);
}

pub export fn gh_delete_thread_subscription(wrapper: *ClientWrapper, thread_id: [*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/notifications/threads/{s}/subscription", .{std.mem.sliceTo(thread_id, 0)}) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    return requestBytesResult(ctx, .DELETE, path, null, null);
}

pub export fn gh_get_repo_notifications(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8, all: c_int, participating: c_int, since: ?[*:0]const u8, before: ?[*:0]const u8, per_page: u8, page: u32) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/notifications", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    const query = buildNotificationQuery(ctx.allocator, all != 0, participating != 0, since, before, page, per_page) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(query);
    return requestBytesResult(ctx, .GET, path, query, null);
}

pub export fn gh_mark_repo_notifications_read(wrapper: *ClientWrapper, owner: [*:0]const u8, repo: [*:0]const u8, last_read_at: ?[*:0]const u8) GhResult {
    wrapper.client.rateLimitDelay();
    const ctx = wrapper.client.apiCtx();
    const path = std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/notifications", .{ std.mem.sliceTo(owner, 0), std.mem.sliceTo(repo, 0) }) catch return .{
        .data = null,
        .error_code = @intFromEnum(GhError.unknown),
    };
    defer ctx.allocator.free(path);
    var body_owned: ?[]u8 = null;
    if (last_read_at) |t| {
        body_owned = std.fmt.allocPrint(ctx.allocator, "{{\"last_read_at\":\"{s}\"}}", .{std.mem.sliceTo(t, 0)}) catch return .{
            .data = null,
            .error_code = @intFromEnum(GhError.unknown),
        };
    }
    defer if (body_owned) |b| ctx.allocator.free(b);
    const body: ?[]const u8 = if (body_owned) |b| b else null;
    return requestBytesResult(ctx, .PUT, path, null, body);
}

// ─────────── Tools ───────────

pub export fn gh_check_rate_limit(wrapper: *ClientWrapper) GhResult {
    wrapper.client.rateLimitDelay();
    return requestBytesResult(wrapper.client.apiCtx(), .GET, "/rate_limit", null, null);
}