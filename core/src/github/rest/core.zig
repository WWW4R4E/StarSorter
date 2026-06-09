const std = @import("std");
const http = std.http;
const types = @import("../types.zig");

pub const ApiContext = struct {
    allocator: std.mem.Allocator,
    client: *http.Client,
    token: []const u8,
};

pub const RawResponse = struct {
    body: []u8,
    status: http.Status,
};

pub fn requestBytes(ctx: ApiContext, method: http.Method, path: []const u8, query: ?[]const u8, request_body: ?[]const u8) !RawResponse {
    const uri_str = if (query) |q|
        try std.fmt.allocPrint(ctx.allocator, "{s}{s}?{s}", .{ types.BaseUrl, path, q })
    else
        try std.fmt.allocPrint(ctx.allocator, "{s}{s}", .{ types.BaseUrl, path });
    defer ctx.allocator.free(uri_str);

    const uri = std.Uri.parse(uri_str) catch return error.NetworkError;

    const auth = try std.fmt.allocPrint(ctx.allocator, "Bearer {s}", .{ctx.token});
    defer ctx.allocator.free(auth);

    var extra_headers: [2]http.Header = undefined;
    extra_headers[0] = .{ .name = "Accept", .value = types.AcceptHeader };
    extra_headers[1] = .{ .name = "X-GitHub-Api-Version", .value = types.ApiVersion };

    var req = ctx.client.request(method, uri, .{
        .headers = .{
            .user_agent = .{ .override = "GitHubStarsManager/1.0" },
            .accept_encoding = .{ .override = "" },
            .authorization = .{ .override = auth },
        },
        .extra_headers = &extra_headers,
        .redirect_behavior = .unhandled,
    }) catch return error.NetworkError;
    defer req.deinit();

    if (request_body) |body| {
        req.transfer_encoding = .{ .content_length = body.len };
        var bw = req.sendBodyUnflushed(&.{}) catch return error.NetworkError;
        bw.writer.writeAll(body) catch return error.NetworkError;
        bw.end() catch return error.NetworkError;
        req.connection.?.flush() catch return error.NetworkError;
    } else {
        req.sendBodiless() catch return error.NetworkError;
    }

    var redirect_buf: [128]u8 = undefined;
    var response = req.receiveHead(&redirect_buf) catch return error.NetworkError;

    const status = response.head.status;

    // Always read the body first, so we can return error messages even on failure
    var transfer_buf: [4096]u8 = undefined;
    const reader = response.reader(&transfer_buf);
    var body = std.ArrayList(u8).empty;
    var buf: [4096]u8 = undefined;
    while (true) {
        const n = reader.readSliceShort(&buf) catch break;
        if (n == 0) break;
        try body.appendSlice(ctx.allocator, buf[0..n]);
    }

    switch (status.class()) {
        .success => return .{ .body = body.items, .status = status },
        .redirect => {
            ctx.allocator.free(body.items);
            return error.HttpError;
        },
        .client_error => {
            if (status == .unauthorized) {
                ctx.allocator.free(body.items);
                return error.Unauthorized;
            }
            if (status == .not_found) {
                ctx.allocator.free(body.items);
                return error.NotFound;
            }
            // For 403 and other non-401/404 client errors: return the body
            // so callers like requestBytesResult can pass the error message through
            return .{ .body = body.items, .status = status };
        },
        .server_error => {
            ctx.allocator.free(body.items);
            return error.ServerError;
        },
        else => {
            ctx.allocator.free(body.items);
            return error.HttpError;
        },
    }
}

pub fn requestJson(ctx: ApiContext, comptime T: type, method: http.Method, path: []const u8, query: ?[]const u8, request_body: ?[]const u8) !T {
    const raw = try requestBytes(ctx, method, path, query, request_body);
    defer ctx.allocator.free(raw.body);

    // If the response is not a success status, return an error
    // (the body was already read but we can't pass it through a Zig error)
    if (raw.status.class() != .success) {
        return error.HttpError;
    }

    return std.json.parseFromSliceLeaky(T, ctx.allocator, raw.body, .{ .ignore_unknown_fields = true }) catch return error.JsonParseFailed;
}

pub fn checkRateLimit(ctx: ApiContext) !types.RateLimit {
    return requestJson(ctx, types.RateLimit, .GET, "/rate_limit", null, null);
}
