#ifndef GITHUB_STARS_H
#define GITHUB_STARS_H

#include <stdint.h>
#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

// ─── Error Codes ───────────────────────────────────────────────

typedef enum {
    GH_SUCCESS        =  0,
    GH_NETWORK_ERROR  = -1,
    GH_UNAUTHORIZED   = -2,
    GH_NOT_FOUND      = -3,
    GH_RATE_LIMITED   = -4,
    GH_SERVER_ERROR   = -5,
    GH_PARSE_ERROR    = -6,
    GH_FORBIDDEN       = -7,
    GH_HTTP_ERROR      = -8,
    GH_UNKNOWN        = -99,
} GhError;

// ─── Result Type ───────────────────────────────────────────────

typedef struct {
    char* data;         // JSON string on success; error message on failure (free with gh_free_string)
    int   error_code;   // 0 = success; negative = GhError code
} GhResult;

// ─── Enums for search parameters ───────────────────────────────

typedef enum {
    GH_PLATFORM_ANDROID = 0,
    GH_PLATFORM_MACOS   = 1,
    GH_PLATFORM_WINDOWS = 2,
    GH_PLATFORM_LINUX   = 3,
    GH_PLATFORM_ALL     = 4,
} GhPlatform;

typedef enum {
    GH_LANGUAGE_KOTLIN     = 0,
    GH_LANGUAGE_JAVA       = 1,
    GH_LANGUAGE_JAVASCRIPT = 2,
    GH_LANGUAGE_TYPESCRIPT = 3,
    GH_LANGUAGE_PYTHON     = 4,
    GH_LANGUAGE_SWIFT      = 5,
    GH_LANGUAGE_RUST       = 6,
    GH_LANGUAGE_GO         = 7,
    GH_LANGUAGE_CSHARP     = 8,
    GH_LANGUAGE_CPLUSPLUS  = 9,
    GH_LANGUAGE_C          = 10,
    GH_LANGUAGE_DART       = 11,
    GH_LANGUAGE_RUBY       = 12,
    GH_LANGUAGE_PHP        = 13,
    GH_LANGUAGE_ALL        = 14,
} GhLanguage;

typedef enum {
    GH_SORT_BEST_MATCH = 0,
    GH_SORT_MOST_STARS = 1,
    GH_SORT_MOST_FORKS = 2,
} GhSortBy;

typedef enum {
    GH_ORDER_DESCENDING = 0,
    GH_ORDER_ASCENDING  = 1,
} GhSortOrder;

typedef enum {
    GH_RANGE_DAILY   = 0,
    GH_RANGE_WEEKLY  = 1,
    GH_RANGE_MONTHLY = 2,
} GhTimeRange;

// ─── Lifecycle ─────────────────────────────────────────────────

/// Create a GitHub API client instance.
/// token    — GitHub Personal Access Token (required).
/// proxy_url — Optional proxy URL (e.g. "http://127.0.0.1:7897"), pass NULL for direct.
/// Returns an opaque handle, or NULL on allocation failure.
/// Call gh_deinit to release.
void* gh_init(const char* token, const char* proxy_url);

/// Destroy a client created by gh_init.
void gh_deinit(void* wrapper);

/// Free a string returned by any gh_* function.
void gh_free_string(char* s);

// ─── User API ──────────────────────────────────────────────────

/// Get the authenticated user's profile (raw GitHub JSON).
GhResult gh_get_current_user(void* wrapper);

/// Get starred repositories (paginated, raw GitHub JSON).
/// page     — 1-based page number.
/// per_page — Items per page (max 100).
GhResult gh_get_starred_repos(void* wrapper, uint32_t page, uint8_t per_page);

/// Get ALL starred repositories (auto-paginated, serialized JSON).
GhResult gh_get_all_starred_repos(void* wrapper);

/// Unstar a repository.
GhResult gh_unstar_repository(void* wrapper, const char* owner, const char* repo);

/// Star a repository.
GhResult gh_star_repository(void* wrapper, const char* owner, const char* repo);

/// Get repositories forked by the authenticated user (raw GitHub JSON).
GhResult gh_get_user_forks(void* wrapper);

// ─── Repository API ────────────────────────────────────────────

/// Get a single repository (raw GitHub JSON).
GhResult gh_get_repository(void* wrapper, const char* owner, const char* repo);

/// Get a repository's README (raw GitHub JSON, base64-encoded content).
/// C# side must decode `content` field from base64.
GhResult gh_get_repository_readme(void* wrapper, const char* owner, const char* repo);

/// Get repository language breakdown (raw GitHub JSON).
/// Returns e.g. {"C#":12345,"C++":67890}.
GhResult gh_get_repo_languages(void* wrapper, const char* owner, const char* repo);

/// Get the latest release (raw GitHub JSON).
GhResult gh_get_latest_release(void* wrapper, const char* owner, const char* repo);

/// Get repository contributors (raw GitHub JSON).
GhResult gh_get_contributors(void* wrapper, const char* owner, const char* repo);

/// Get repository forks (raw GitHub JSON).
/// sort — "newest", "oldest", or "stargazers".
GhResult gh_get_forks(void* wrapper, const char* owner, const char* repo, const char* sort, uint8_t per_page);

/// Get recent commits (raw GitHub JSON).
GhResult gh_get_commits(void* wrapper, const char* owner, const char* repo, uint8_t per_page, uint32_t page);

/// Get repository tags (raw GitHub JSON).
GhResult gh_get_tags(void* wrapper, const char* owner, const char* repo);

/// Check if the authenticated user has starred a repository.
/// Returns "true" or "false" string.
GhResult gh_check_starred(void* wrapper, const char* owner, const char* repo);

/// Get an organization's profile (raw GitHub JSON).
GhResult gh_get_organization(void* wrapper, const char* org);

// ─── Release API ───────────────────────────────────────────────

/// Get repository releases (paginated, raw GitHub JSON).
GhResult gh_get_releases(void* wrapper, const char* owner, const char* repo, uint32_t page, uint8_t per_page);

/// Get ALL releases for a repository (auto-paginated, serialized JSON).
GhResult gh_fetch_all_releases(void* wrapper, const char* owner, const char* repo);

/// Get releases published after a given Unix timestamp (serialized JSON).
GhResult gh_get_incremental_releases(void* wrapper, const char* owner, const char* repo, int64_t since);

// ─── Search API ────────────────────────────────────────────────

/// Search repositories with more than 1000 stars (serialized JSON).
GhResult gh_search_most_stars(void* wrapper, uint32_t page, uint8_t per_page);

/// Search repositories with more than 1000 forks (serialized JSON).
GhResult gh_search_most_forks(void* wrapper, uint32_t page, uint8_t per_page);

/// Multi-condition repository search (serialized JSON).
GhResult gh_search_repositories(void* wrapper, const char* query, int platform, int language, int sort, int order, uint32_t page, uint8_t per_page);

/// Search developers with >1000 followers (serialized JSON).
GhResult gh_search_daily_devs(void* wrapper, uint32_t page, uint8_t per_page);

/// Search repositories by topic keyword (serialized JSON).
GhResult gh_search_by_topic(void* wrapper, const char* topic, uint32_t page, uint8_t per_page);

/// Search AI/ML topic repositories (serialized JSON).
GhResult gh_get_topic_repos(void* wrapper, uint32_t page, uint8_t per_page);

/// Search recently active high-star repositories (serialized JSON).
GhResult gh_get_hot_release_repos(void* wrapper, uint32_t page, uint8_t per_page);

/// Search most popular repositories (>10000 stars, serialized JSON).
GhResult gh_get_most_popular(void* wrapper, uint32_t page, uint8_t per_page);

/// Search trending repositories (serialized JSON).
GhResult gh_search_trending(void* wrapper, int language, int range);

// ─── Fork API ──────────────────────────────────────────────────

/// Sync a fork with its upstream (raw GitHub JSON).
GhResult gh_sync_fork(void* wrapper, const char* owner, const char* repo);

/// Check if a fork needs syncing (raw GitHub JSON).
GhResult gh_check_fork_sync(void* wrapper, const char* owner, const char* repo, const char* branch);

/// Get repository branches (raw GitHub JSON).
GhResult gh_get_branches(void* wrapper, const char* owner, const char* repo);

// ─── Actions API ───────────────────────────────────────────────

/// Get GitHub Actions workflows (serialized JSON, unwrapped from envelope).
GhResult gh_get_workflows(void* wrapper, const char* owner, const char* repo);

/// Trigger a GitHub Actions workflow run (204 No Content).
GhResult gh_trigger_workflow(void* wrapper, const char* owner, const char* repo, const char* workflow_path, const char* ref);

// ─── Notifications API ─────────────────────────────────────────

/// List notifications for the authenticated user (raw GitHub JSON).
/// all           — if true, show notifications marked as read (0/1).
/// participating — if true, only show participating notifications (0/1).
/// since         — ISO 8601 timestamp; only results updated after this time (pass NULL to skip).
/// before        — ISO 8601 timestamp; only results updated before this time (pass NULL to skip).
/// page          — 1-based page number.
/// per_page      — Items per page (max 50).
GhResult gh_get_notifications(void* wrapper, int all, int participating, const char* since, const char* before, uint32_t page, uint8_t per_page);

/// Mark all notifications as read (raw GitHub JSON response).
/// last_read_at — ISO 8601 timestamp; pass NULL to mark all as read.
GhResult gh_mark_notifications_read(void* wrapper, const char* last_read_at);

/// Get a single notification thread (raw GitHub JSON).
GhResult gh_get_thread(void* wrapper, const char* thread_id);

/// Mark a thread as read (205 Reset Content).
GhResult gh_mark_thread_read(void* wrapper, const char* thread_id);

/// Mark a thread as done (204 No Content).
GhResult gh_mark_thread_done(void* wrapper, const char* thread_id);

/// Get a thread subscription (raw GitHub JSON).
GhResult gh_get_thread_subscription(void* wrapper, const char* thread_id);

/// Set a thread subscription (raw GitHub JSON).
/// ignored — if non-zero, block all notifications from this thread.
GhResult gh_set_thread_subscription(void* wrapper, const char* thread_id, int ignored);

/// Delete a thread subscription (204 No Content).
GhResult gh_delete_thread_subscription(void* wrapper, const char* thread_id);

/// List repository notifications for the authenticated user (raw GitHub JSON).
GhResult gh_get_repo_notifications(void* wrapper, const char* owner, const char* repo, int all, int participating, const char* since, const char* before, uint8_t per_page, uint32_t page);

/// Mark repository notifications as read (raw GitHub JSON response).
GhResult gh_mark_repo_notifications_read(void* wrapper, const char* owner, const char* repo, const char* last_read_at);

// ─── Tools ─────────────────────────────────────────────────────

/// Get current API rate limit status (JSON).
GhResult gh_check_rate_limit(void* wrapper);

#ifdef __cplusplus
}
#endif

#endif // GITHUB_STARS_H
