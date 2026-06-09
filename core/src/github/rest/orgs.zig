const std = @import("std");
const core = @import("core.zig");
const types = @import("../types.zig");

pub fn getOrganization(ctx: core.ApiContext, org: []const u8) !types.Organization {
    const path = try std.fmt.allocPrint(ctx.allocator, "/orgs/{s}", .{org});
    defer ctx.allocator.free(path);
    return core.requestJson(ctx, types.Organization, .GET, path, null, null);
}
