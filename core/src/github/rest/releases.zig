const std = @import("std");
const core = @import("core.zig");
const types = @import("../types.zig");

pub fn getRepositoryReleases(ctx: core.ApiContext, owner: []const u8, repo: []const u8, page: u32, per_page: u8) ![]types.Release {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/releases", .{ owner, repo });
    defer ctx.allocator.free(path);
    const query = try std.fmt.allocPrint(ctx.allocator, "page={d}&per_page={d}", .{ page, per_page });
    defer ctx.allocator.free(query);
    return core.requestJson(ctx, []types.Release, .GET, path, query, null);
}
