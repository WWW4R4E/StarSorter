const std = @import("std");
const core = @import("core.zig");
const types = @import("../types.zig");

pub fn getRepositoryWorkflows(ctx: core.ApiContext, owner: []const u8, repo: []const u8) ![]types.WorkflowDefinition {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/actions/workflows", .{ owner, repo });
    defer ctx.allocator.free(path);
    const result = try core.requestJson(ctx, struct {
        total_count: u32,
        workflows: []types.WorkflowDefinition,
    }, .GET, path, null, null);
    return result.workflows;
}

pub fn triggerWorkflowRun(ctx: core.ApiContext, owner: []const u8, repo: []const u8, workflow_path: []const u8, ref: []const u8) !void {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/actions/workflows/{s}/dispatches", .{ owner, repo, workflow_path });
    defer ctx.allocator.free(path);
    const body = try std.fmt.allocPrint(ctx.allocator, "{{\"ref\":\"{s}\"}}", .{ref});
    defer ctx.allocator.free(body);
    _ = try core.requestBytes(ctx, .POST, path, null, body);
}
