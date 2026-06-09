const std = @import("std");
const core = @import("core.zig");
const types = @import("../types.zig");

pub fn getCurrentUser(ctx: core.ApiContext) !types.GitHubUser {
    return core.requestJson(ctx, types.GitHubUser, .GET, "/user", null, null);
}

pub fn getStarredRepositories(ctx: core.ApiContext, page: u32, per_page: u8) ![]types.Repository {
    const query = try std.fmt.allocPrint(ctx.allocator, "page={d}&per_page={d}&sort=created&direction=desc", .{ page, per_page });
    defer ctx.allocator.free(query);
    return core.requestJson(ctx, []types.Repository, .GET, "/user/starred", query, null);
}

pub fn unstarRepository(ctx: core.ApiContext, owner: []const u8, repo: []const u8) !void {
    const path = try std.fmt.allocPrint(ctx.allocator, "/user/starred/{s}/{s}", .{ owner, repo });
    defer ctx.allocator.free(path);
    _ = try core.requestBytes(ctx, .DELETE, path, null, null);
}

pub fn starRepository(ctx: core.ApiContext, owner: []const u8, repo: []const u8) !void {
    const path = try std.fmt.allocPrint(ctx.allocator, "/user/starred/{s}/{s}", .{ owner, repo });
    defer ctx.allocator.free(path);
    _ = try core.requestBytes(ctx, .PUT, path, null, null);
}

pub fn getUserForks(ctx: core.ApiContext) ![]types.Repository {
    return core.requestJson(ctx, []types.Repository, .GET, "/user/repos", "type=forks&sort=updated&per_page=100", null);
}

pub fn getUserByLogin(ctx: core.ApiContext, login: []const u8) !types.GitHubUser {
    const path = try std.fmt.allocPrint(ctx.allocator, "/users/{s}", .{login});
    defer ctx.allocator.free(path);
    return core.requestJson(ctx, types.GitHubUser, .GET, path, null, null);
}
