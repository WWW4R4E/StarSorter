const std = @import("std");
const core = @import("core.zig");
const types = @import("../types.zig");

pub fn syncFork(ctx: core.ApiContext, owner: []const u8, repo: []const u8) !types.SyncForkResult {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/merge-upstream", .{ owner, repo });
    defer ctx.allocator.free(path);
    const body = try std.fmt.allocPrint(ctx.allocator, "{{\"branch\":\"main\"}}", .{});
    defer ctx.allocator.free(body);
    return core.requestJson(ctx, types.SyncForkResult, .POST, path, null, body);
}

pub fn checkForkSyncNeeded(ctx: core.ApiContext, owner: []const u8, repo: []const u8, branch: []const u8) !types.CompareResult {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/compare/{s}", .{ owner, repo, branch });
    defer ctx.allocator.free(path);
    return core.requestJson(ctx, types.CompareResult, .GET, path, null, null);
}
