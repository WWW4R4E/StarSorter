const std = @import("std");
const core = @import("core.zig");
const types = @import("../types.zig");

fn buildSearchQuery(allocator: std.mem.Allocator, query: []const u8, sort: types.SortBy, order: types.SortOrder, page: u32, per_page: u8) ![]u8 {
    const sort_str = switch (sort) {
        .BestMatch => "",
        .MostStars => "&sort=stars",
        .MostForks => "&sort=forks",
    };
    const order_str = switch (order) {
        .Descending => "&order=desc",
        .Ascending => "&order=asc",
    };
    return std.fmt.allocPrint(allocator, "q={s}{s}{s}&page={d}&per_page={d}", .{ query, sort_str, order_str, page, per_page });
}

pub fn searchRepos(ctx: core.ApiContext, query: []const u8, sort: types.SortBy, order: types.SortOrder, page: u32, per_page: u8) !types.SearchResults(types.Repository) {
    const q = try buildSearchQuery(ctx.allocator, query, sort, order, page, per_page);
    defer ctx.allocator.free(q);
    return core.requestJson(ctx, types.SearchResults(types.Repository), .GET, "/search/repositories", q, null);
}

pub fn searchMostStars(ctx: core.ApiContext, page: u32, per_page: u8) !types.SearchResults(types.Repository) {
    return searchRepos(ctx, "stars:>1000", .MostStars, .Descending, page, per_page);
}

pub fn searchMostForks(ctx: core.ApiContext, page: u32, per_page: u8) !types.SearchResults(types.Repository) {
    return searchRepos(ctx, "forks:>1000", .MostForks, .Descending, page, per_page);
}

pub fn searchRepositories(ctx: core.ApiContext, query: []const u8, platform: types.Platform, language: types.Language, sort: types.SortBy, order: types.SortOrder, page: u32, per_page: u8) !types.SearchResults(types.Repository) {
    var buf = std.ArrayList(u8).empty;
    try buf.appendSlice(ctx.allocator, query);

    if (language != .All) {
        try buf.appendSlice(ctx.allocator, "+language:");
        try buf.appendSlice(ctx.allocator, language.queryString().?);
    }
    if (platform != .All) {
        try buf.appendSlice(ctx.allocator, "+topic:");
        try buf.appendSlice(ctx.allocator, @tagName(platform));
    }

    return searchRepos(ctx, buf.items, sort, order, page, per_page);
}

pub fn searchByTopic(ctx: core.ApiContext, topic_keywords: []const []const u8, page: u32, per_page: u8) !types.SearchResults(types.Repository) {
    var query = std.ArrayList(u8).empty;
    for (topic_keywords, 0..) |kw, i| {
        if (i > 0) try query.appendSlice(ctx.allocator, "+");
        try query.appendSlice(ctx.allocator, "topic:");
        try query.appendSlice(ctx.allocator, kw);
    }
    return searchRepos(ctx, query.items, .MostStars, .Descending, page, per_page);
}

pub fn getTopicRepositories(ctx: core.ApiContext, page: u32, per_page: u8) !types.SearchResults(types.Repository) {
    return searchByTopic(ctx, &.{ "artificial-intelligence", "machine-learning", "ai" }, page, per_page);
}

pub fn getMostPopular(ctx: core.ApiContext, page: u32, per_page: u8) !types.SearchResults(types.Repository) {
    return searchRepos(ctx, "stars:>10000", .MostStars, .Descending, page, per_page);
}

pub fn searchTrending(ctx: core.ApiContext, language: types.Language, range: types.TimeRange) !types.SearchResults(types.Repository) {
    _ = range;
    const lang_str = if (language != .All)
        try std.fmt.allocPrint(ctx.allocator, "+language:{s}", .{language.queryString().?})
    else
        "";
    defer if (language != .All) ctx.allocator.free(lang_str);

    const query = try std.fmt.allocPrint(ctx.allocator, "stars:>100{s}+pushed:>2025-01-01", .{lang_str});
    defer ctx.allocator.free(query);
    return searchRepos(ctx, query, .MostStars, .Descending, 1, 30);
}
