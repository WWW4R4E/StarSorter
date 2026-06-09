const std = @import("std");
const core = @import("core.zig");
const types = @import("../types.zig");

fn buildNotificationQuery(ctx: core.ApiContext, all: bool, participating: bool, since: ?[]const u8, before: ?[]const u8, page: u32, per_page: u8) ![]const u8 {
    var buf = std.ArrayList(u8).empty;
    var num_buf: [32]u8 = undefined;
    try buf.appendSlice(ctx.allocator, "page=");
    try buf.appendSlice(ctx.allocator, try std.fmt.bufPrint(&num_buf, "{d}", .{page}));
    try buf.appendSlice(ctx.allocator, "&per_page=");
    try buf.appendSlice(ctx.allocator, try std.fmt.bufPrint(&num_buf, "{d}", .{per_page}));
    if (all) try buf.appendSlice(ctx.allocator, "&all=true");
    if (participating) try buf.appendSlice(ctx.allocator, "&participating=true");
    if (since) |s| {
        try buf.appendSlice(ctx.allocator, "&since=");
        try buf.appendSlice(ctx.allocator, s);
    }
    if (before) |b| {
        try buf.appendSlice(ctx.allocator, "&before=");
        try buf.appendSlice(ctx.allocator, b);
    }
    return buf.items;
}

pub fn getNotifications(ctx: core.ApiContext, all: bool, participating: bool, since: ?[]const u8, before: ?[]const u8, page: u32, per_page: u8) ![]types.NotificationThread {
    const query = try buildNotificationQuery(ctx, all, participating, since, before, page, per_page);
    defer ctx.allocator.free(query);
    return core.requestJson(ctx, []types.NotificationThread, .GET, "/notifications", query, null);
}

pub fn markNotificationsRead(ctx: core.ApiContext, last_read_at: ?[]const u8) !types.MarkNotificationsResponse {
    var body: ?[]u8 = null;
    if (last_read_at) |t| {
        body = try std.fmt.allocPrint(ctx.allocator, "{{\"last_read_at\":\"{s}\"}}", .{t});
    }
    defer if (body) |b| ctx.allocator.free(b);
    return core.requestJson(ctx, types.MarkNotificationsResponse, .PUT, "/notifications", null, body);
}

pub fn getThread(ctx: core.ApiContext, thread_id: []const u8) !types.NotificationThread {
    const path = try std.fmt.allocPrint(ctx.allocator, "/notifications/threads/{s}", .{thread_id});
    defer ctx.allocator.free(path);
    return core.requestJson(ctx, types.NotificationThread, .GET, path, null, null);
}

pub fn markThreadRead(ctx: core.ApiContext, thread_id: []const u8) !void {
    const path = try std.fmt.allocPrint(ctx.allocator, "/notifications/threads/{s}", .{thread_id});
    defer ctx.allocator.free(path);
    _ = try core.requestBytes(ctx, .PATCH, path, null, null);
}

pub fn markThreadDone(ctx: core.ApiContext, thread_id: []const u8) !void {
    const path = try std.fmt.allocPrint(ctx.allocator, "/notifications/threads/{s}", .{thread_id});
    defer ctx.allocator.free(path);
    _ = try core.requestBytes(ctx, .DELETE, path, null, null);
}

pub fn getThreadSubscription(ctx: core.ApiContext, thread_id: []const u8) !types.ThreadSubscription {
    const path = try std.fmt.allocPrint(ctx.allocator, "/notifications/threads/{s}/subscription", .{thread_id});
    defer ctx.allocator.free(path);
    return core.requestJson(ctx, types.ThreadSubscription, .GET, path, null, null);
}

pub fn setThreadSubscription(ctx: core.ApiContext, thread_id: []const u8, ignored: bool) !types.ThreadSubscription {
    const path = try std.fmt.allocPrint(ctx.allocator, "/notifications/threads/{s}/subscription", .{thread_id});
    defer ctx.allocator.free(path);
    const body: []const u8 = if (ignored) "{\"ignored\":true}" else "{\"ignored\":false}";
    return core.requestJson(ctx, types.ThreadSubscription, .PUT, path, null, body);
}

pub fn deleteThreadSubscription(ctx: core.ApiContext, thread_id: []const u8) !void {
    const path = try std.fmt.allocPrint(ctx.allocator, "/notifications/threads/{s}/subscription", .{thread_id});
    defer ctx.allocator.free(path);
    _ = try core.requestBytes(ctx, .DELETE, path, null, null);
}

pub fn getRepoNotifications(ctx: core.ApiContext, owner: []const u8, repo: []const u8, all: bool, participating: bool, since: ?[]const u8, before: ?[]const u8, per_page: u8, page: u32) ![]types.NotificationThread {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/notifications", .{ owner, repo });
    defer ctx.allocator.free(path);
    const query = try buildNotificationQuery(ctx, all, participating, since, before, page, per_page);
    defer ctx.allocator.free(query);
    return core.requestJson(ctx, []types.NotificationThread, .GET, path, query, null);
}

pub fn markRepoNotificationsRead(ctx: core.ApiContext, owner: []const u8, repo: []const u8, last_read_at: ?[]const u8) !types.MarkNotificationsResponse {
    const path = try std.fmt.allocPrint(ctx.allocator, "/repos/{s}/{s}/notifications", .{ owner, repo });
    defer ctx.allocator.free(path);
    var body: ?[]u8 = null;
    if (last_read_at) |t| {
        body = try std.fmt.allocPrint(ctx.allocator, "{{\"last_read_at\":\"{s}\"}}", .{t});
    }
    defer if (body) |b| ctx.allocator.free(b);
    return core.requestJson(ctx, types.MarkNotificationsResponse, .PUT, path, null, body);
}
