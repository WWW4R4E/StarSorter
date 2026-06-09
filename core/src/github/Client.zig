const std = @import("std");
const http = std.http;
const Io = std.Io;
const Allocator = std.mem.Allocator;

const rest = struct {
    pub const core = @import("rest/core.zig");
    pub const users = @import("rest/users.zig");
    pub const repos = @import("rest/repos.zig");
    pub const releases = @import("rest/releases.zig");
    pub const search = @import("rest/search.zig");
    pub const forks = @import("rest/forks.zig");
    pub const actions = @import("rest/actions.zig");
    pub const orgs = @import("rest/orgs.zig");
    pub const notifications = @import("rest/notifications.zig");
};
const types = @import("types.zig");
const GitHubUser = types.GitHubUser;
const Repository = types.Repository;
const Release = types.Release;
const WorkflowDefinition = types.WorkflowDefinition;
const Branch = types.Branch;
const Contributor = types.Contributor;
const Commit = types.Commit;
const Tag = types.Tag;
const Organization = types.Organization;
const RateLimit = types.RateLimit;
const NotificationThread = types.NotificationThread;
const ThreadSubscription = types.ThreadSubscription;
const MarkNotificationsResponse = types.MarkNotificationsResponse;
const SearchResults = types.SearchResults;
const SyncForkResult = types.SyncForkResult;
const CompareResult = types.CompareResult;
const Language = types.Language;
const SortBy = types.SortBy;
const SortOrder = types.SortOrder;

pub const Error = error{
    HttpError,
    JsonParseFailed,
    RateLimitExceeded,
    Canceled,
    NotFound,
    Unauthorized,
    ServerError,
    NetworkError,
} || std.mem.Allocator.Error || http.Client.RequestError || http.Client.FetchError;

pub const GitHubClient = struct {
    http_client: http.Client,
    allocator: Allocator,
    io: Io,
    token: []const u8,
    base_url: []const u8 = types.BaseUrl,

    rate_limit_remaining: u32 = 0,
    rate_limit_reset: u64 = 0,
    last_request_ns: i96 = 0,
    min_interval_ns: i96 = 100_000_000,

    pub fn init(allocator: Allocator, io: Io, token: []const u8) GitHubClient {
        return .{
            .http_client = .{ .allocator = allocator, .io = io },
            .allocator = allocator,
            .io = io,
            .token = token,
        };
    }

    pub fn initProxies(self: *GitHubClient, proxy_url: ?[]const u8) !void {
        const url = proxy_url orelse return;
        const uri = try std.Uri.parse(url);
        const host = try uri.getHostAlloc(self.allocator);
        const proxy = try self.allocator.create(http.Client.Proxy);
        proxy.* = .{
            .protocol = .plain,
            .host = host,
            .authorization = null,
            .port = uri.port orelse 80,
            .supports_connect = false,
        };
        self.http_client.http_proxy = proxy;
        self.http_client.https_proxy = proxy;
    }

    pub fn deinit(self: *GitHubClient) void {
        self.http_client.deinit();
        self.* = undefined;
    }

    pub fn rateLimitDelay(self: *GitHubClient) void {
        const now_ns = Io.Timestamp.now(self.io, .real).nanoseconds;
        const elapsed = now_ns - self.last_request_ns;
        if (elapsed < self.min_interval_ns) {
            Io.sleep(self.io, .{ .nanoseconds = self.min_interval_ns - elapsed }, .real) catch {};
        }
        self.last_request_ns = now_ns;
    }

    pub fn apiCtx(self: *GitHubClient) rest.core.ApiContext {
        return .{ .allocator = self.allocator, .client = &self.http_client, .token = self.token };
    }

    // ─────────── User API ───────────

    pub fn getCurrentUser(self: *GitHubClient) Error!GitHubUser {
        self.rateLimitDelay();
        return rest.users.getCurrentUser(self.apiCtx());
    }

    pub fn getStarredRepositories(self: *GitHubClient, page: u32, per_page: u8) Error![]Repository {
        self.rateLimitDelay();
        return rest.users.getStarredRepositories(self.apiCtx(), page, per_page);
    }

    pub fn getAllStarredRepositories(self: *GitHubClient) Error![]Repository {
        var all = std.ArrayList(Repository).empty;
        var page: u32 = 1;
        const per_page: u8 = 100;
        while (true) {
            const repos = try self.getStarredRepositories(page, per_page);
            if (repos.len == 0) break;
            try all.appendSlice(self.allocator, repos);
            page += 1;
        }
        return all.items;
    }

    pub fn unstarRepository(self: *GitHubClient, owner: []const u8, repo: []const u8) Error!void {
        self.rateLimitDelay();
        return rest.users.unstarRepository(self.apiCtx(), owner, repo);
    }

    pub fn starRepository(self: *GitHubClient, owner: []const u8, repo: []const u8) Error!void {
        self.rateLimitDelay();
        return rest.users.starRepository(self.apiCtx(), owner, repo);
    }

    pub fn getUserForks(self: *GitHubClient) Error![]Repository {
        self.rateLimitDelay();
        return rest.users.getUserForks(self.apiCtx());
    }

    // ─────────── Repository API ───────────

    pub fn getRepository(self: *GitHubClient, owner: []const u8, repo: []const u8) Error!Repository {
        self.rateLimitDelay();
        return rest.repos.getRepository(self.apiCtx(), owner, repo);
    }

    pub fn getRepositoryReadme(self: *GitHubClient, owner: []const u8, repo: []const u8) Error![]u8 {
        self.rateLimitDelay();
        return rest.repos.getRepositoryReadme(self.apiCtx(), owner, repo);
    }

    pub fn getRepositoryLanguages(self: *GitHubClient, owner: []const u8, repo: []const u8) Error![]u8 {
        self.rateLimitDelay();
        return rest.repos.getRepositoryLanguages(self.apiCtx(), owner, repo);
    }

    pub fn getLatestRelease(self: *GitHubClient, owner: []const u8, repo: []const u8) Error!Release {
        self.rateLimitDelay();
        return rest.repos.getLatestRelease(self.apiCtx(), owner, repo);
    }

    pub fn getContributors(self: *GitHubClient, owner: []const u8, repo: []const u8) Error![]Contributor {
        self.rateLimitDelay();
        return rest.repos.getContributors(self.apiCtx(), owner, repo);
    }

    pub fn getForks(self: *GitHubClient, owner: []const u8, repo: []const u8, sort: []const u8, per_page: u8) Error![]Repository {
        self.rateLimitDelay();
        return rest.repos.getForks(self.apiCtx(), owner, repo, sort, per_page);
    }

    pub fn getCommits(self: *GitHubClient, owner: []const u8, repo: []const u8, per_page: u8, page: u32) Error![]Commit {
        self.rateLimitDelay();
        return rest.repos.getCommits(self.apiCtx(), owner, repo, per_page, page);
    }

    pub fn getTags(self: *GitHubClient, owner: []const u8, repo: []const u8) Error![]Tag {
        self.rateLimitDelay();
        return rest.repos.getTags(self.apiCtx(), owner, repo);
    }

    pub fn checkStarred(self: *GitHubClient, owner: []const u8, repo: []const u8) Error!bool {
        self.rateLimitDelay();
        return rest.repos.checkStarred(self.apiCtx(), owner, repo);
    }

    pub fn getOrganization(self: *GitHubClient, org: []const u8) Error!Organization {
        self.rateLimitDelay();
        return rest.orgs.getOrganization(self.apiCtx(), org);
    }

    // ─────────── Release API ───────────

    pub fn getRepositoryReleases(self: *GitHubClient, owner: []const u8, repo: []const u8, page: u32, per_page: u8) Error![]Release {
        self.rateLimitDelay();
        return rest.releases.getRepositoryReleases(self.apiCtx(), owner, repo, page, per_page);
    }

    pub fn fetchAllReleasesForRepo(self: *GitHubClient, owner: []const u8, repo: []const u8) Error![]Release {
        var all = std.ArrayList(Release).empty;
        var page: u32 = 1;
        const per_page: u8 = 100;
        while (true) {
            const releases = try self.getRepositoryReleases(owner, repo, page, per_page);
            if (releases.len == 0) break;
            try all.appendSlice(self.allocator, releases);
            page += 1;
        }
        return all.items;
    }

    pub fn getIncrementalRepositoryReleases(self: *GitHubClient, owner: []const u8, repo: []const u8, since: i64) Error![]Release {
        const all = try self.fetchAllReleasesForRepo(owner, repo);
        var filtered = std.ArrayList(Release).empty;
        for (all) |rel| {
            if (rel.published_at) |published| {
                const timestamp = parseRfc3339(published) catch continue;
                if (timestamp > since) {
                    try filtered.append(self.allocator, rel);
                }
            }
        }
        return filtered.items;
    }

    fn parseRfc3339(date_str: []const u8) !i64 {
        if (date_str.len < 20) return error.InvalidDate;
        const year = try std.fmt.parseInt(i64, date_str[0..4], 10);
        const month = try std.fmt.parseInt(i64, date_str[5..7], 10);
        const day = try std.fmt.parseInt(i64, date_str[8..10], 10);
        const hour = try std.fmt.parseInt(i64, date_str[11..13], 10);
        const minute = try std.fmt.parseInt(i64, date_str[14..16], 10);
        const second = try std.fmt.parseInt(i64, date_str[17..19], 10);

        const days_before_month = [_]i64{ 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
        const y = year - 1;
        var total_days = y * 365 + @divFloor(y, 4) - @divFloor(y, 100) + @divFloor(y, 400);
        total_days += days_before_month[@as(usize, @intCast(month - 1))];
        if (std.time.epoch.isLeapYear(@as(u16, @intCast(year))) and month > 2) total_days += 1;
        total_days += day - 1;

        return (total_days - 719468) * 86400 + hour * 3600 + minute * 60 + second;
    }

    // ─────────── Search API ───────────

    pub fn searchMostStars(self: *GitHubClient, page: u32, per_page: u8) Error!SearchResults(Repository) {
        self.rateLimitDelay();
        return rest.search.searchMostStars(self.apiCtx(), page, per_page);
    }

    pub fn searchMostForks(self: *GitHubClient, page: u32, per_page: u8) Error!SearchResults(Repository) {
        self.rateLimitDelay();
        return rest.search.searchMostForks(self.apiCtx(), page, per_page);
    }

    pub fn searchRepositories(
        self: *GitHubClient,
        query: []const u8,
        platform: types.Platform,
        language: Language,
        sort: SortBy,
        order: SortOrder,
        page: u32,
        per_page: u8,
    ) Error!SearchResults(Repository) {
        self.rateLimitDelay();
        return rest.search.searchRepositories(self.apiCtx(), query, platform, language, sort, order, page, per_page);
    }

    pub fn searchDailyDevs(self: *GitHubClient, page: u32, per_page: u8) Error![]GitHubUser {
        self.rateLimitDelay();
        const ctx = self.apiCtx();
        const q = try std.fmt.allocPrint(ctx.allocator, "q=followers:>1000&page={d}&per_page={d}&sort=followers&order=desc", .{ page, per_page });
        defer ctx.allocator.free(q);
        const results = try rest.core.requestJson(ctx, SearchResults(GitHubUser), .GET, "/search/users", q, null);
        var detailed = std.ArrayList(GitHubUser).empty;
        for (results.items) |user| {
            const full = rest.users.getUserByLogin(ctx, user.login) catch continue;
            try detailed.append(ctx.allocator, full);
        }
        return detailed.items;
    }

    pub fn searchByTopic(self: *GitHubClient, topic_keywords: []const []const u8, page: u32, per_page: u8) Error!SearchResults(Repository) {
        self.rateLimitDelay();
        return rest.search.searchByTopic(self.apiCtx(), topic_keywords, page, per_page);
    }

    pub fn getTopicRepositories(self: *GitHubClient, page: u32, per_page: u8) Error!SearchResults(Repository) {
        self.rateLimitDelay();
        return rest.search.getTopicRepositories(self.apiCtx(), page, per_page);
    }

    pub fn getHotReleaseRepositories(self: *GitHubClient, page: u32, per_page: u8) Error!SearchResults(Repository) {
        self.rateLimitDelay();
        const ctx = self.apiCtx();
        const now = @divFloor(Io.Timestamp.now(self.io, .real).nanoseconds, 1_000_000_000);
        const two_weeks: i64 = 14 * 24 * 60 * 60;
        const since = now - two_weeks;
        const query = try std.fmt.allocPrint(ctx.allocator, "pushed:>{d}+stars:>100", .{since});
        defer ctx.allocator.free(query);
        return rest.search.searchRepos(ctx, query, .MostStars, .Descending, page, per_page);
    }

    pub fn getMostPopular(self: *GitHubClient, page: u32, per_page: u8) Error!SearchResults(Repository) {
        self.rateLimitDelay();
        return rest.search.getMostPopular(self.apiCtx(), page, per_page);
    }

    pub fn searchTrending(self: *GitHubClient, language: Language, range: types.TimeRange) Error!SearchResults(Repository) {
        self.rateLimitDelay();
        return rest.search.searchTrending(self.apiCtx(), language, range);
    }

    // ─────────── Fork API ───────────

    pub fn syncFork(self: *GitHubClient, owner: []const u8, repo: []const u8) Error!SyncForkResult {
        self.rateLimitDelay();
        return rest.forks.syncFork(self.apiCtx(), owner, repo);
    }

    pub fn checkForkSyncNeeded(self: *GitHubClient, owner: []const u8, repo: []const u8, branch: []const u8) Error!CompareResult {
        self.rateLimitDelay();
        return rest.forks.checkForkSyncNeeded(self.apiCtx(), owner, repo, branch);
    }

    pub fn getBranches(self: *GitHubClient, owner: []const u8, repo: []const u8) Error![]Branch {
        self.rateLimitDelay();
        return rest.repos.getBranches(self.apiCtx(), owner, repo);
    }

    // ─────────── Actions API ───────────

    pub fn getRepositoryWorkflows(self: *GitHubClient, owner: []const u8, repo: []const u8) Error![]WorkflowDefinition {
        self.rateLimitDelay();
        return rest.actions.getRepositoryWorkflows(self.apiCtx(), owner, repo);
    }

    pub fn triggerWorkflowRun(self: *GitHubClient, owner: []const u8, repo: []const u8, workflow_path: []const u8, ref: []const u8) Error!void {
        self.rateLimitDelay();
        return rest.actions.triggerWorkflowRun(self.apiCtx(), owner, repo, workflow_path, ref);
    }

    // ─────────── Notifications API ───────────

    pub fn getNotifications(self: *GitHubClient, all: bool, participating: bool, since: ?[]const u8, before: ?[]const u8, page: u32, per_page: u8) Error![]NotificationThread {
        self.rateLimitDelay();
        return rest.notifications.getNotifications(self.apiCtx(), all, participating, since, before, page, per_page);
    }

    pub fn markNotificationsRead(self: *GitHubClient, last_read_at: ?[]const u8) Error!MarkNotificationsResponse {
        self.rateLimitDelay();
        return rest.notifications.markNotificationsRead(self.apiCtx(), last_read_at);
    }

    pub fn getThread(self: *GitHubClient, thread_id: []const u8) Error!NotificationThread {
        self.rateLimitDelay();
        return rest.notifications.getThread(self.apiCtx(), thread_id);
    }

    pub fn markThreadRead(self: *GitHubClient, thread_id: []const u8) Error!void {
        self.rateLimitDelay();
        return rest.notifications.markThreadRead(self.apiCtx(), thread_id);
    }

    pub fn markThreadDone(self: *GitHubClient, thread_id: []const u8) Error!void {
        self.rateLimitDelay();
        return rest.notifications.markThreadDone(self.apiCtx(), thread_id);
    }

    pub fn getThreadSubscription(self: *GitHubClient, thread_id: []const u8) Error!ThreadSubscription {
        self.rateLimitDelay();
        return rest.notifications.getThreadSubscription(self.apiCtx(), thread_id);
    }

    pub fn setThreadSubscription(self: *GitHubClient, thread_id: []const u8, ignored: bool) Error!ThreadSubscription {
        self.rateLimitDelay();
        return rest.notifications.setThreadSubscription(self.apiCtx(), thread_id, ignored);
    }

    pub fn deleteThreadSubscription(self: *GitHubClient, thread_id: []const u8) Error!void {
        self.rateLimitDelay();
        return rest.notifications.deleteThreadSubscription(self.apiCtx(), thread_id);
    }

    pub fn getRepositoryNotifications(self: *GitHubClient, owner: []const u8, repo: []const u8, all: bool, participating: bool, since: ?[]const u8, before: ?[]const u8, per_page: u8, page: u32) Error![]NotificationThread {
        self.rateLimitDelay();
        return rest.notifications.getRepoNotifications(self.apiCtx(), owner, repo, all, participating, since, before, per_page, page);
    }

    pub fn markRepositoryNotificationsRead(self: *GitHubClient, owner: []const u8, repo: []const u8, last_read_at: ?[]const u8) Error!MarkNotificationsResponse {
        self.rateLimitDelay();
        return rest.notifications.markRepoNotificationsRead(self.apiCtx(), owner, repo, last_read_at);
    }

    // ─────────── Tools ───────────

    pub fn checkRateLimit(self: *GitHubClient) Error!RateLimit {
        self.rateLimitDelay();
        return rest.core.checkRateLimit(self.apiCtx());
    }
};
