const std = @import("std");
const core = @import("core");
const friendlyError = core.friendlyError;
const GitHubUser = core.types.GitHubUser;
const UserPlan = core.types.UserPlan;

pub fn main(init: std.process.Init) !void {
    const arena: std.mem.Allocator = init.arena.allocator();
    const io = init.io;

    // 从环境变量 GITHUB_TOKEN 读取 token
    const token = init.environ_map.get("GITHUB_TOKEN") orelse {
        std.debug.print("错误: 请设置 GITHUB_TOKEN 环境变量\n", .{});
        return;
    };

    // 初始化 GitHub 客户端
    var gh = core.Client.init(arena, io, token);
    // 代理通过 gh_init 的 proxy_url 参数设置
    defer gh.deinit();

    if (gh.getRepository("duanebester", "gooey")) |repo| {
        std.debug.print("✓ 仓库信息: {s}\n", .{repo.full_name});
    } else |err| {
        std.debug.print("✗ 获取仓库失败: {s}\n", .{friendlyError(err)});
    }

    // 通过 C ABI 导出函数测试（模拟 C# 互操作）
    {
        const wrapper = core.gh_init(@ptrCast(token.ptr), null) orelse {
            std.debug.print("✗ gh_init 失败\n", .{});
            return;
        };
        defer core.gh_deinit(wrapper);

        const result = core.gh_get_repository(wrapper, "duanebester", "gooey");
        defer if (result.data) |d| core.gh_free_string(d);

        if (result.error_code == 0) {
            std.debug.print("✓ C ABI gh_get_repository 成功: {?s}\n", .{result.data});
        } else {
            std.debug.print("✗ C ABI gh_get_repository 失败: error={d}\n", .{result.error_code});
        }
    }
}
