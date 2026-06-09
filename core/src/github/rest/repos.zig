const std = @import("std");
const core = @import("core.zig");
const types = @import("../types.zig");

pub fn getRepository(ctx: core.ApiContext, owner: []const u8, repo: []const u8) !types.Repository {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}", .{ owner, repo });
    defer ctx.allocator.free(path);
    return core.requestJson(ctx, types.Repository, .GET, path, null, null);
}

pub fn getRepositoryLanguages(ctx: core.ApiContext, owner: []const u8, repo: []const u8) ![]u8 {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/languages", .{ owner, repo });
    defer ctx.allocator.free(path);
    const result = try core.requestBytes(ctx, .GET, path, null, null);
    return result.body;
}

pub fn getRepositoryReadme(ctx: core.ApiContext, owner: []const u8, repo: []const u8) ![]u8 {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/readme", .{ owner, repo });
    defer ctx.allocator.free(path);

    const raw = try core.requestBytes(ctx, .GET, path, null, null);
    const parsed = std.json.parseFromSliceLeaky(struct {
        content: []const u8,
        encoding: []const u8,
    }, ctx.allocator, raw.body, .{ .ignore_unknown_fields = true }) catch return error.JsonParseFailed;

    if (!std.ascii.eqlIgnoreCase(parsed.encoding, "base64")) return error.JsonParseFailed;
    const decoded_len = std.base64.standard.Decoder.calcSizeForSlice(parsed.content) catch return error.JsonParseFailed;
    const decoded = try ctx.allocator.alloc(u8, decoded_len);
    std.base64.standard.Decoder.decode(decoded, parsed.content) catch return error.JsonParseFailed;
    return decoded;
}

pub fn getLatestRelease(ctx: core.ApiContext, owner: []const u8, repo: []const u8) !types.Release {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/releases/latest", .{ owner, repo });
    defer ctx.allocator.free(path);
    return core.requestJson(ctx, types.Release, .GET, path, null, null);
}

pub fn getContributors(ctx: core.ApiContext, owner: []const u8, repo: []const u8) ![]types.Contributor {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/contributors", .{ owner, repo });
    defer ctx.allocator.free(path);
    return core.requestJson(ctx, []types.Contributor, .GET, path, null, null);
}

pub fn getForks(ctx: core.ApiContext, owner: []const u8, repo: []const u8, sort: []const u8, per_page: u8) ![]types.Repository {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/forks", .{ owner, repo });
    defer ctx.allocator.free(path);
    const query = try std.fmt.allocPrint(ctx.allocator, "sort={s}&per_page={d}", .{ sort, per_page });
    defer ctx.allocator.free(query);
    return core.requestJson(ctx, []types.Repository, .GET, path, query, null);
}

pub fn getCommits(ctx: core.ApiContext, owner: []const u8, repo: []const u8, per_page: u8, page: u32) ![]types.Commit {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/commits", .{ owner, repo });
    defer ctx.allocator.free(path);
    const query = try std.fmt.allocPrint(ctx.allocator, "per_page={d}&page={d}", .{ per_page, page });
    defer ctx.allocator.free(query);
    return core.requestJson(ctx, []types.Commit, .GET, path, query, null);
}

pub fn getTags(ctx: core.ApiContext, owner: []const u8, repo: []const u8) ![]types.Tag {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/tags", .{ owner, repo });
    defer ctx.allocator.free(path);
    return core.requestJson(ctx, []types.Tag, .GET, path, null, null);
}

pub fn checkStarred(ctx: core.ApiContext, owner: []const u8, repo: []const u8) !bool {
    const path = try std.fmt.allocPrint(ctx.allocator, "/user/starred/{s}/{s}", .{ owner, repo });
    defer ctx.allocator.free(path);
    const result = core.requestBytes(ctx, .GET, path, null, null) catch |err| {
        if (err == error.NotFound) return false;
        return err;
    };
    ctx.allocator.free(result.body);
    return true;
}

pub fn getBranches(ctx: core.ApiContext, owner: []const u8, repo: []const u8) ![]types.Branch {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/branches", .{ owner, repo });
    defer ctx.allocator.free(path);
    return core.requestJson(ctx, []types.Branch, .GET, path, null, null);
}
