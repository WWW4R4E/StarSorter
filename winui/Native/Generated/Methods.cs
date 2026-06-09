using System.Runtime.InteropServices;

namespace StarSorter.Native
{
    public static unsafe partial class Methods
    {
        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void* gh_init([NativeTypeName("const char *")] sbyte* token, [NativeTypeName("const char *")] sbyte* proxy_url);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void gh_deinit(void* wrapper);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void gh_free_string([NativeTypeName("char *")] sbyte* s);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_current_user(void* wrapper);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_starred_repos(void* wrapper, [NativeTypeName("uint32_t")] uint page, [NativeTypeName("uint8_t")] byte per_page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_all_starred_repos(void* wrapper);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_unstar_repository(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_star_repository(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_user_forks(void* wrapper);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_repository(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_repository_readme(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_repo_languages(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_latest_release(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_contributors(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_forks(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo, [NativeTypeName("const char *")] sbyte* sort, [NativeTypeName("uint8_t")] byte per_page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_commits(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo, [NativeTypeName("uint8_t")] byte per_page, [NativeTypeName("uint32_t")] uint page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_tags(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_check_starred(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_organization(void* wrapper, [NativeTypeName("const char *")] sbyte* org);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_releases(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo, [NativeTypeName("uint32_t")] uint page, [NativeTypeName("uint8_t")] byte per_page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_fetch_all_releases(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_incremental_releases(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo, [NativeTypeName("int64_t")] long since);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_search_most_stars(void* wrapper, [NativeTypeName("uint32_t")] uint page, [NativeTypeName("uint8_t")] byte per_page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_search_most_forks(void* wrapper, [NativeTypeName("uint32_t")] uint page, [NativeTypeName("uint8_t")] byte per_page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_search_repositories(void* wrapper, [NativeTypeName("const char *")] sbyte* query, int platform, int language, int sort, int order, [NativeTypeName("uint32_t")] uint page, [NativeTypeName("uint8_t")] byte per_page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_search_daily_devs(void* wrapper, [NativeTypeName("uint32_t")] uint page, [NativeTypeName("uint8_t")] byte per_page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_search_by_topic(void* wrapper, [NativeTypeName("const char *")] sbyte* topic, [NativeTypeName("uint32_t")] uint page, [NativeTypeName("uint8_t")] byte per_page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_topic_repos(void* wrapper, [NativeTypeName("uint32_t")] uint page, [NativeTypeName("uint8_t")] byte per_page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_hot_release_repos(void* wrapper, [NativeTypeName("uint32_t")] uint page, [NativeTypeName("uint8_t")] byte per_page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_most_popular(void* wrapper, [NativeTypeName("uint32_t")] uint page, [NativeTypeName("uint8_t")] byte per_page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_search_trending(void* wrapper, int language, int range);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_sync_fork(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_check_fork_sync(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo, [NativeTypeName("const char *")] sbyte* branch);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_branches(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_workflows(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_trigger_workflow(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo, [NativeTypeName("const char *")] sbyte* workflow_path, [NativeTypeName("const char *")] sbyte* @ref);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_notifications(void* wrapper, int all, int participating, [NativeTypeName("const char *")] sbyte* since, [NativeTypeName("const char *")] sbyte* before, [NativeTypeName("uint32_t")] uint page, [NativeTypeName("uint8_t")] byte per_page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_mark_notifications_read(void* wrapper, [NativeTypeName("const char *")] sbyte* last_read_at);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_thread(void* wrapper, [NativeTypeName("const char *")] sbyte* thread_id);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_mark_thread_read(void* wrapper, [NativeTypeName("const char *")] sbyte* thread_id);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_mark_thread_done(void* wrapper, [NativeTypeName("const char *")] sbyte* thread_id);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_thread_subscription(void* wrapper, [NativeTypeName("const char *")] sbyte* thread_id);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_set_thread_subscription(void* wrapper, [NativeTypeName("const char *")] sbyte* thread_id, int ignored);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_delete_thread_subscription(void* wrapper, [NativeTypeName("const char *")] sbyte* thread_id);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_get_repo_notifications(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo, int all, int participating, [NativeTypeName("const char *")] sbyte* since, [NativeTypeName("const char *")] sbyte* before, [NativeTypeName("uint8_t")] byte per_page, [NativeTypeName("uint32_t")] uint page);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_mark_repo_notifications_read(void* wrapper, [NativeTypeName("const char *")] sbyte* owner, [NativeTypeName("const char *")] sbyte* repo, [NativeTypeName("const char *")] sbyte* last_read_at);

        [DllImport("github_stars.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern GhResult gh_check_rate_limit(void* wrapper);
    }
}
