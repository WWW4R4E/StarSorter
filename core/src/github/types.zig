//! GitHub API v3 数据模型。
//!
//! 字段名与 GitHub JSON 键完全匹配（snake_case），
//! 以便通过 `std.json.parseFromSlice` 直接反序列化。

const std = @import("std");

pub const ApiVersion = "2022-11-28";
pub const AcceptHeader = "application/vnd.github.v3+json";
pub const BaseUrl = "https://api.github.com";

/// GitHub 用户账户。
pub const GitHubUser = struct {
    login: []const u8,
    id: u64,
    node_id: ?[]const u8 = null,
    avatar_url: ?[]const u8 = null,
    gravatar_id: ?[]const u8 = null,
    html_url: ?[]const u8 = null,
    name: ?[]const u8 = null,
    company: ?[]const u8 = null,
    blog: ?[]const u8 = null,
    location: ?[]const u8 = null,
    email: ?[]const u8 = null,
    bio: ?[]const u8 = null,
    twitter_username: ?[]const u8 = null,
    public_repos: ?u32 = null,
    public_gists: ?u32 = null,
    followers: ?u32 = null,
    following: ?u32 = null,
    created_at: ?[]const u8 = null,
    updated_at: ?[]const u8 = null,
    /// 仅在已认证用户中存在。
    plan: ?UserPlan = null,
};

/// GitHub 用户订阅计划（在已认证用户中存在）。
pub const UserPlan = struct {
    /// 订阅计划名称（例如 "free"、"pro"）。
    name: []const u8,
    /// 总磁盘空间（以字节为单位）。
    space: u64,
    /// 最大协作者数量。
    collaborators: u32,
    /// 最大私有仓库数量。
    private_repos: u32,
};

/// 仓库安全与分析设置。
pub const SecurityAnalysisStatus = struct {
    status: []const u8,
    enabled: ?bool = null,
};

pub const SecurityAndAnalysis = struct {
    advanced_security: ?SecurityAnalysisStatus = null,
    secret_scanning: ?SecurityAnalysisStatus = null,
    secret_scanning_push_protection: ?SecurityAnalysisStatus = null,
};

/// GitHub 仓库。
pub const Repository = struct {
    id: u64,
    node_id: ?[]const u8 = null,
    name: []const u8,
    full_name: []const u8,
    private: bool = false,
    owner: GitHubUser,
    html_url: []const u8,
    description: ?[]const u8 = null,
    fork: bool = false,
    url: []const u8,
    created_at: []const u8,
    updated_at: []const u8,
    pushed_at: ?[]const u8 = null,
    homepage: ?[]const u8 = null,
    size: ?u64 = null,
    stargazers_count: u32 = 0,
    watchers_count: u32 = 0,
    language: ?[]const u8 = null,
    forks_count: u32 = 0,
    open_issues_count: u32 = 0,
    topics: ?[]const []const u8 = null,
    visibility: ?[]const u8 = null,
    default_branch: []const u8 = "main",
    parent: ?RepositoryRef = null,
    source: ?RepositoryRef = null,
    license: ?License = null,
    subscribers_count: u32 = 0,
    archived: bool = false,
    security_and_analysis: ?SecurityAndAnalysis = null,
};

/// 对另一个仓库的引用（fork 的父仓库/源仓库）。
pub const RepositoryRef = struct {
    id: u64,
    node_id: ?[]const u8 = null,
    name: []const u8,
    full_name: []const u8,
    owner: GitHubUser,
    html_url: []const u8,
};

/// 仓库许可证信息。
pub const License = struct {
    key: []const u8,
    name: []const u8,
    spdx_id: ?[]const u8 = null,
    url: ?[]const u8 = null,
    node_id: ?[]const u8 = null,
};

/// 版本发布。
pub const Release = struct {
    id: u64,
    tag_name: []const u8,
    target_commitish: []const u8 = "main",
    name: ?[]const u8 = null,
    body: ?[]const u8 = null,
    draft: bool = false,
    prerelease: bool = false,
    created_at: []const u8,
    published_at: ?[]const u8 = null,
    html_url: []const u8,
    assets: []ReleaseAsset = &.{},
    tarball_url: ?[]const u8 = null,
    zipball_url: ?[]const u8 = null,
};

pub const ReleaseAsset = struct {
    id: u64,
    name: []const u8,
    content_type: []const u8,
    size: u64,
    browser_download_url: []const u8,
    created_at: []const u8,
    updated_at: []const u8,
};

/// GitHub Actions 工作流定义。
pub const WorkflowDefinition = struct {
    id: u64,
    node_id: ?[]const u8 = null,
    name: []const u8,
    path: []const u8,
    state: []const u8,
    created_at: []const u8,
    updated_at: []const u8,
};

/// 仓库分支。
pub const Branch = struct {
    name: []const u8,
    commit: BranchCommit,
    protected: bool = false,
};

pub const BranchCommit = struct {
    sha: []const u8,
    url: []const u8,
};

/// API 速率限制状态。
pub const RateLimit = struct {
    resources: RateLimitResources,
};

pub const RateLimitResources = struct {
    core: RateLimitItem,
    search: RateLimitItem,
    graphql: RateLimitItem,
};

pub const RateLimitItem = struct {
    limit: u32,
    remaining: u32,
    reset: u64,
    used: u32,
};

/// GitHub 搜索 API 响应的包装器。
pub fn SearchResults(comptime T: type) type {
    return struct {
        total_count: u64,
        incomplete_results: bool = false,
        items: []T = &.{},
    };
}

/// 同步 fork 结果（向上游合并）。
pub const SyncForkResult = struct {
    status: []const u8,
    message: []const u8,
};

/// 用于 fork 同步检查的比较结果。
pub const CompareResult = struct {
    status: []const u8,
    ahead_by: u32,
    behind_by: u32,
    total_commits: u32,
    files: []CompareFile = &.{},
};

pub const CompareFile = struct {
    sha: []const u8,
    filename: []const u8,
    status: []const u8,
};

/// 工作流触发请求体。
pub const WorkflowDispatchBody = struct {
    ref: []const u8,
    inputs: ?[]const u8 = null,
};

/// 仓库贡献者。
pub const Contributor = struct {
    login: []const u8,
    id: u64,
    avatar_url: []const u8,
    html_url: []const u8,
    contributions: u32,
    type: []const u8,
};

/// Git 提交记录。
pub const Commit = struct {
    sha: []const u8,
    commit: CommitDetails,
    author: ?ContributorBrief = null,
    committer: ?ContributorBrief = null,
};

pub const CommitDetails = struct {
    message: []const u8,
    author: CommitAuthor,
    committer: CommitAuthor,
};

pub const CommitAuthor = struct {
    name: []const u8,
    email: []const u8,
    date: []const u8,
};

pub const ContributorBrief = struct {
    login: ?[]const u8 = null,
    id: ?u64 = null,
    avatar_url: ?[]const u8 = null,
    html_url: ?[]const u8 = null,
    type: ?[]const u8 = null,
};

/// 仓库标签（tag）。
pub const Tag = struct {
    name: []const u8,
    commit: TagCommitRef,
    zipball_url: []const u8,
    tarball_url: []const u8,
};

pub const TagCommitRef = struct {
    sha: []const u8,
    url: []const u8,
};

/// GitHub 组织。
// ───── 通知类型 ─────

/// 通知线程主题（Issue / PR / Commit / Release 等）。
pub const NotificationSubject = struct {
    title: []const u8,
    url: []const u8,
    latest_comment_url: []const u8,
    type: []const u8,
};

/// GitHub 通知线程。
pub const NotificationThread = struct {
    id: []const u8,
    repository: Repository,
    subject: NotificationSubject,
    reason: []const u8,
    unread: bool,
    updated_at: []const u8,
    last_read_at: ?[]const u8 = null,
    url: []const u8,
    subscription_url: []const u8,
};

/// 线程订阅信息。
pub const ThreadSubscription = struct {
    subscribed: bool,
    ignored: bool,
    reason: ?[]const u8 = null,
    created_at: ?[]const u8 = null,
    url: []const u8,
    thread_url: ?[]const u8 = null,
    repository_url: ?[]const u8 = null,
};

/// 标记通知为已读的响应。
pub const MarkNotificationsResponse = struct {
    message: []const u8,
    url: ?[]const u8 = null,
};

pub const Organization = struct {
    login: []const u8,
    id: u64,
    node_id: ?[]const u8 = null,
    avatar_url: ?[]const u8 = null,
    html_url: ?[]const u8 = null,
    name: ?[]const u8 = null,
    company: ?[]const u8 = null,
    blog: ?[]const u8 = null,
    location: ?[]const u8 = null,
    email: ?[]const u8 = null,
    description: ?[]const u8 = null,
    public_repos: ?u32 = null,
    public_gists: ?u32 = null,
    followers: ?u32 = null,
    following: ?u32 = null,
    created_at: ?[]const u8 = null,
    updated_at: ?[]const u8 = null,
};

// ───── 搜索参数枚举 ─────

pub const Platform = enum {
    Android,
    Macos,
    Windows,
    Linux,
    All,
};

pub const Language = enum {
    Kotlin,
    Java,
    JavaScript,
    TypeScript,
    Python,
    Swift,
    Rust,
    Go,
    CSharp,
    CPlusPlus,
    C,
    Dart,
    Ruby,
    PHP,
    All,

    pub fn queryString(lang: Language) ?[]const u8 {
        return switch (lang) {
            .CPlusPlus => "C%2B%2B",
            .CSharp => "C%23",
            .All => null,
            else => @tagName(lang),
        };
    }
};

pub const SortBy = enum {
    BestMatch,
    MostStars,
    MostForks,
};

pub const SortOrder = enum {
    Descending,
    Ascending,
};

pub const TimeRange = enum {
    daily,
    weekly,
    monthly,
};

// ───── 发现频道 ─────

pub const DiscoveryChannel = enum {
    most_stars,
    most_forks,
    trending,
    hot_release,
    most_popular,
    topic,
    search,

    pub fn label(ch: DiscoveryChannel) []const u8 {
        return switch (ch) {
            .most_stars => "most-stars",
            .most_forks => "most-forks",
            .trending => "trending",
            .hot_release => "hot-release",
            .most_popular => "most-popular",
            .topic => "topic",
            .search => "search",
        };
    }
};
