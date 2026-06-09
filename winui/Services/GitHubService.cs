using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Microsoft.Windows.Storage;
using StarSorter.Helpers;
using StarSorter.Models;
using StarSorter.Native;
using StarSorter.ViewModels;

namespace StarSorter.Services
{
    public unsafe class GitHubService
    {
        private readonly ILogger<GitHubService> _logger;

        private static bool _proxyInitialized;

        public GitHubService(ILogger<GitHubService> logger)
        {
            _logger = logger;
            AutoDetectProxy();
        }

        private void* CreateAndInit()
        {
            var token = ResolveToken();
            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException("GitHub token not configured. Set GH_TOKEN or GITHUB_TOKEN environment variable, or enter your token in Settings.");

            var proxyUrl = ResolveProxyUrl();
            _logger.LogDebug("CreateAndInit: token={Token}, proxy={Proxy}",
                token[..Math.Min(4, token.Length)] + "***",
                proxyUrl ?? "null");

            var tokenBytes = Encoding.UTF8.GetBytes(token + "\0");
            fixed (byte* pToken = tokenBytes)
            {
                if (!string.IsNullOrEmpty(proxyUrl))
                {
                    var proxyBytes = Encoding.UTF8.GetBytes(proxyUrl + "\0");
                    fixed (byte* pProxy = proxyBytes)
                    {
                        var wrapper = Methods.gh_init((sbyte*)pToken, (sbyte*)pProxy);
                        _logger.LogDebug("CreateAndInit: wrapper=0x{(ulong)wrapper:X}", (ulong)wrapper);
                        if (wrapper == null)
                            throw new InvalidOperationException("GitHub API initialization failed");
                        return wrapper;
                    }
                }
                else
                {
                    var wrapper = Methods.gh_init((sbyte*)pToken, null);
                    _logger.LogDebug("CreateAndInit: wrapper=0x{(ulong)wrapper:X}", (ulong)wrapper);
                    if (wrapper == null)
                        throw new InvalidOperationException("GitHub API initialization failed");
                    return wrapper;
                }
            }
        }

        public void SaveToken(string token)
        {
            var settings = ApplicationData.GetDefault().LocalSettings;
            settings.Values[SettingsKeys.GitHubToken] = token;
        }

        public string? GetSavedToken()
        {
            return ResolveToken();
        }

        private static string? ResolveToken()
        {
            var settings = ApplicationData.GetDefault().LocalSettings;
            if (settings.Values.TryGetValue(SettingsKeys.GitHubToken, out var saved) && saved is string savedToken && !string.IsNullOrEmpty(savedToken))
                return savedToken;

            foreach (var envVar in new[] { "GH_TOKEN", "GITHUB_TOKEN" })
            {
                var envToken = Environment.GetEnvironmentVariable(envVar);
                if (!string.IsNullOrEmpty(envToken))
                {
                    settings.Values[SettingsKeys.GitHubToken] = envToken;
                    return envToken;
                }
            }

            return null;
        }

        private static void AutoDetectProxy()
        {
            if (_proxyInitialized)
                return;

            var settings = ApplicationData.GetDefault().LocalSettings;
            if (settings.Values.ContainsKey(SettingsKeys.GitHubProxyMode))
            {
                _proxyInitialized = true;
                return;
            }

            foreach (var envVar in new[] { "HTTPS_PROXY", "HTTP_PROXY", "ALL_PROXY", "https_proxy", "http_proxy", "all_proxy" })
            {
                var value = Environment.GetEnvironmentVariable(envVar);
                if (!string.IsNullOrEmpty(value))
                {
                    settings.Values[SettingsKeys.GitHubProxyUrl] = value;
                    settings.Values[SettingsKeys.GitHubProxyMode] = nameof(ProxyMode.System);
                    _proxyInitialized = true;
                    System.Diagnostics.Debug.WriteLine($"[AutoDetectProxy] Detected proxy from {envVar}: {value}");
                    return;
                }
            }

            settings.Values[SettingsKeys.GitHubProxyMode] = nameof(ProxyMode.Disabled);
            _proxyInitialized = true;
            System.Diagnostics.Debug.WriteLine("[AutoDetectProxy] No proxy detected, set to Disabled");
        }

        public ProxyMode GetProxyMode()
        {
            var settings = ApplicationData.GetDefault().LocalSettings;
            if (settings.Values.TryGetValue(SettingsKeys.GitHubProxyMode, out var saved) && saved is string modeStr && !string.IsNullOrEmpty(modeStr))
            {
                if (Enum.TryParse<ProxyMode>(modeStr, out var mode))
                    return mode;
            }
            return ProxyMode.Disabled;
        }

        public void SaveProxyMode(ProxyMode mode)
        {
            var settings = ApplicationData.GetDefault().LocalSettings;
            settings.Values[SettingsKeys.GitHubProxyMode] = mode.ToString();
        }

        public string? GetCustomProxyUrl()
        {
            var settings = ApplicationData.GetDefault().LocalSettings;
            if (settings.Values.TryGetValue(SettingsKeys.GitHubProxyUrl, out var saved) && saved is string url && !string.IsNullOrEmpty(url))
                return url;
            return null;
        }

        public void SaveCustomProxyUrl(string url)
        {
            var settings = ApplicationData.GetDefault().LocalSettings;
            settings.Values[SettingsKeys.GitHubProxyUrl] = url;
        }

        private string? ResolveProxyUrl()
        {
            switch (GetProxyMode())
            {
                case ProxyMode.Disabled:
                    return null;
                case ProxyMode.System:
                    foreach (var envVar in new[] { "HTTPS_PROXY", "HTTP_PROXY", "ALL_PROXY", "https_proxy", "http_proxy", "all_proxy" })
                    {
                        var value = Environment.GetEnvironmentVariable(envVar);
                        if (!string.IsNullOrEmpty(value))
                            return value;
                    }
                    return null;
                case ProxyMode.Custom:
                    return GetCustomProxyUrl();
                default:
                    return null;
            }
        }

        private string MarshalString(GhResult result)
        {
            _logger.LogDebug("error_code={ErrorCode}, data_ptr=0x{DataPtr:X}", result.error_code, (ulong)result.data);
            string? serverMessage = null;
            if (result.data != null)
            {
                serverMessage = new string(result.data);
                Methods.gh_free_string(result.data);
            }

            if (result.error_code != 0)
            {
                _logger.LogDebug("server_message: '{ServerMessage}'", serverMessage ?? "null");
                throw new GitHubApiException((GhError)result.error_code, serverMessage);
            }

            _logger.LogDebug("data (len={Length}): '{Data}'", serverMessage?.Length ?? 0,
                serverMessage is { Length: > 0 } ? serverMessage[..Math.Min(serverMessage.Length, 500)] + (serverMessage.Length > 500 ? "..." : "") : "null");

            return serverMessage ?? string.Empty;
        }

        private T Deserialize<T>(GhResult result) where T : new()
        {
            var json = MarshalString(result);
            try
            {
                return (T)(JsonSerializer.Deserialize(json, SerializationContext.SnakeCaseLower.GetTypeInfo(typeof(T))!) ?? new T());
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialize failed for {Type}", typeof(T).Name);
                _logger.LogDebug("Raw JSON (first 200): '{Json}'", json[..Math.Min(json.Length, 200)]);
                throw;
            }
        }

        private List<T> DeserializeList<T>(GhResult result)
        {
            var json = MarshalString(result);
            try
            {
                return (List<T>)(JsonSerializer.Deserialize(json, SerializationContext.SnakeCaseLower.GetTypeInfo(typeof(List<T>))!) ?? new List<T>());
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialize list failed for {Type}", typeof(T).Name);
                _logger.LogDebug("Raw JSON (first 200): '{Json}'", json[..Math.Min(json.Length, 200)]);
                throw;
            }
        }

        // ===== User API =====

        public Task<GitHubUser> GetCurrentUserAsync()
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    var result = Methods.gh_get_current_user(wrapper);
                    return Deserialize<GitHubUser>(result);
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<RateLimitResponse> CheckRateLimitAsync()
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    var result = Methods.gh_check_rate_limit(wrapper);
                    return Deserialize<RateLimitResponse>(result);
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        // ===== Stars API =====

        public Task<List<Repository>> GetAllStarredReposAsync()
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    var result = Methods.gh_get_all_starred_repos(wrapper);
                    try
                    {
                        return DeserializeList<Repository>(result);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "gh_get_all_starred_repos returned malformed JSON, falling back to paginated mode");
                        return GetAllStarredReposPaginatedAsync(wrapper);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        private List<Repository> GetAllStarredReposPaginatedAsync(void* wrapper)
        {
            var all = new List<Repository>();
            uint page = 1;
            const byte perPage = 100;

            while (true)
            {
                var result = Methods.gh_get_starred_repos(wrapper, page, perPage);
                var json = MarshalString(result);

                if (string.IsNullOrWhiteSpace(json) || json.Trim() == "[]")
                    break;

                try
                {
                    var repos = JsonSerializer.Deserialize(json, SerializationContext.SnakeCaseLower.ListRepository);
                    if (repos == null || repos.Count == 0)
                        break;

                    _logger.LogDebug("Page {Page}: got {Count} repos", page, repos.Count);
                    all.AddRange(repos);
                    page++;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize page {Page} of starred repos", page);
                    _logger.LogDebug("Raw JSON length: {Length}, first 200: '{Json}'", json.Length, json[..Math.Min(json.Length, 200)]);
                    page++;
                    continue;
                }
            }

            _logger.LogInformation("Fetched {Total} starred repos via paginated fallback", all.Count);
            return all;
        }

        public Task<List<Repository>> GetStarredReposAsync(uint page, byte perPage = 100)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    var result = Methods.gh_get_starred_repos(wrapper, page, perPage);
                    return DeserializeList<Repository>(result);
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task StarRepositoryAsync(string owner, string repo)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_star_repository(wrapper, (sbyte*)pOwner, (sbyte*)pRepo);
                        MarshalString(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task UnstarRepositoryAsync(string owner, string repo)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_unstar_repository(wrapper, (sbyte*)pOwner, (sbyte*)pRepo);
                        MarshalString(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<bool> CheckStarredAsync(string owner, string repo)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_check_starred(wrapper, (sbyte*)pOwner, (sbyte*)pRepo);
                        if (result.error_code == 0)
                        {
                            var data = MarshalString(result);
                            return bool.TryParse(data, out var starred) && starred;
                        }
                        return false;
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        // ===== Repository API =====

        public Task<Repository> GetRepositoryAsync(string owner, string repo)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    _logger.LogDebug("GetRepositoryAsync: owner='{Owner}', repo='{Repo}'", owner, repo);
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_get_repository(wrapper, (sbyte*)pOwner, (sbyte*)pRepo);
                        _logger.LogDebug("gh_get_repository: error_code={ErrorCode}, data_ptr=0x{DataPtr:X}", result.error_code, (ulong)result.data);
                        var repoObj = Deserialize<Repository>(result);
                        _logger.LogDebug("Deserialized repo: id={Id}, name={Name}", repoObj.Id, repoObj.Name);
                        return repoObj;
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<List<Contributor>> GetContributorsAsync(string owner, string repo)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_get_contributors(wrapper, (sbyte*)pOwner, (sbyte*)pRepo);
                        _logger.LogDebug("GetContributorsAsync: error_code={ErrorCode}", result.error_code);
                        return DeserializeList<Contributor>(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<Release> GetLatestReleaseAsync(string owner, string repo)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_get_latest_release(wrapper, (sbyte*)pOwner, (sbyte*)pRepo);
                        _logger.LogDebug("GetLatestReleaseAsync: error_code={ErrorCode}", result.error_code);
                        return Deserialize<Release>(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<List<Release>> GetReleasesAsync(string owner, string repo, uint page = 1, byte perPage = 30)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_get_releases(wrapper, (sbyte*)pOwner, (sbyte*)pRepo, page, perPage);
                        _logger.LogDebug("GetReleasesAsync: error_code={ErrorCode}", result.error_code);
                        return DeserializeList<Release>(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<string> GetRepositoryReadmeAsync(string owner, string repo)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_get_repository_readme(wrapper, (sbyte*)pOwner, (sbyte*)pRepo);
                        _logger.LogDebug("GetRepositoryReadmeAsync: error_code={ErrorCode}", result.error_code);
                        var json = MarshalString(result);
                        var parsed = JsonSerializer.Deserialize(json, SerializationContext.Default.JsonElement);
                        var base64 = parsed.GetProperty("content").GetString() ?? string.Empty;
                        var bytes = Convert.FromBase64String(base64);
                        return Encoding.UTF8.GetString(bytes);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        // ===== Forks API =====

        public Task<List<Repository>> GetForksAsync(string owner, string repo, string sort = "stargazers", byte perPage = 30)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    fixed (byte* pSort = Encoding.UTF8.GetBytes(sort + "\0"))
                    {
                        var result = Methods.gh_get_forks(wrapper, (sbyte*)pOwner, (sbyte*)pRepo, (sbyte*)pSort, perPage);
                        _logger.LogDebug("GetForksAsync: error_code={ErrorCode}", result.error_code);
                        return DeserializeList<Repository>(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<List<Repository>> GetUserForksAsync()
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    var result = Methods.gh_get_user_forks(wrapper);
                    _logger.LogDebug("GetUserForksAsync: error_code={ErrorCode}", result.error_code);
                    return DeserializeList<Repository>(result);
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        // ===== Sync & Branch API =====

        public Task SyncForkAsync(string owner, string repo)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_sync_fork(wrapper, (sbyte*)pOwner, (sbyte*)pRepo);
                        MarshalString(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<bool> CheckForkSyncAsync(string owner, string repo, string branch)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    fixed (byte* pBranch = Encoding.UTF8.GetBytes(branch + "\0"))
                    {
                        var result = Methods.gh_check_fork_sync(wrapper, (sbyte*)pOwner, (sbyte*)pRepo, (sbyte*)pBranch);
                        _logger.LogDebug("CheckForkSyncAsync: error_code={ErrorCode}", result.error_code);
                        var data = MarshalString(result);
                        return bool.TryParse(data, out var synced) && synced;
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<List<string>> GetBranchesAsync(string owner, string repo)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_get_branches(wrapper, (sbyte*)pOwner, (sbyte*)pRepo);
                        _logger.LogDebug("GetBranchesAsync: error_code={ErrorCode}", result.error_code);
                        var json = MarshalString(result);
                        var branches = JsonSerializer.Deserialize(json, SerializationContext.Default.ListJsonElement);
                        return branches?.Select(b => b.GetProperty("name").GetString() ?? string.Empty).ToList() ?? new();
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        // ===== Workflow API =====

        public Task<List<Workflow>> GetWorkflowsAsync(string owner, string repo)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_get_workflows(wrapper, (sbyte*)pOwner, (sbyte*)pRepo);
                        _logger.LogDebug("GetWorkflowsAsync: error_code={ErrorCode}", result.error_code);
                        return DeserializeList<Workflow>(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task TriggerWorkflowAsync(string owner, string repo, string workflowPath, string @ref)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    fixed (byte* pWorkflowPath = Encoding.UTF8.GetBytes(workflowPath + "\0"))
                    fixed (byte* pRef = Encoding.UTF8.GetBytes(@ref + "\0"))
                    {
                        var result = Methods.gh_trigger_workflow(wrapper, (sbyte*)pOwner, (sbyte*)pRepo, (sbyte*)pWorkflowPath, (sbyte*)pRef);
                        MarshalString(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        // ===== Search API =====

        public Task<List<Repository>> SearchMostStarsAsync(uint page = 1, byte perPage = 30)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    var result = Methods.gh_search_most_stars(wrapper, page, perPage);
                    _logger.LogDebug("SearchMostStarsAsync: error_code={ErrorCode}", result.error_code);
                    var json = MarshalString(result);
                    return JsonSerializer.Deserialize(json, SerializationContext.SnakeCaseLower.SearchResultsRepository)?.Items ?? new();
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<List<Repository>> SearchMostForksAsync(uint page = 1, byte perPage = 30)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    var result = Methods.gh_search_most_forks(wrapper, page, perPage);
                    _logger.LogDebug("SearchMostForksAsync: error_code={ErrorCode}", result.error_code);
                    var json = MarshalString(result);
                    return JsonSerializer.Deserialize(json, SerializationContext.SnakeCaseLower.SearchResultsRepository)?.Items ?? new();
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<List<Repository>> SearchDailyDevsAsync(uint page = 1, byte perPage = 30)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    var result = Methods.gh_search_daily_devs(wrapper, page, perPage);
                    _logger.LogDebug("SearchDailyDevsAsync: error_code={ErrorCode}", result.error_code);
                    return Deserialize<SearchResults<Repository>>(result).Items;
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<List<Repository>> SearchByTopicAsync(string topic, uint page = 1, byte perPage = 30)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pTopic = Encoding.UTF8.GetBytes(topic + "\0"))
                    {
                        var result = Methods.gh_search_by_topic(wrapper, (sbyte*)pTopic, page, perPage);
                        _logger.LogDebug("SearchByTopicAsync: error_code={ErrorCode}", result.error_code);
                        return Deserialize<SearchResults<Repository>>(result).Items;
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<List<Repository>> GetTopicReposAsync(uint page = 1, byte perPage = 30)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    var result = Methods.gh_get_topic_repos(wrapper, page, perPage);
                    _logger.LogDebug("GetTopicReposAsync: error_code={ErrorCode}", result.error_code);
                    return Deserialize<SearchResults<Repository>>(result).Items;
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<List<Repository>> SearchRepositoriesAsync(string query, GhPlatform platform = GhPlatform.GH_PLATFORM_ALL, GhLanguage language = GhLanguage.GH_LANGUAGE_ALL, GhSortBy sort = GhSortBy.GH_SORT_BEST_MATCH, GhSortOrder order = GhSortOrder.GH_ORDER_DESCENDING, uint page = 1, byte perPage = 30)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pQuery = Encoding.UTF8.GetBytes(query + "\0"))
                    {
                        var result = Methods.gh_search_repositories(wrapper, (sbyte*)pQuery, (int)platform, (int)language, (int)sort, (int)order, page, perPage);
                        _logger.LogDebug("SearchRepositoriesAsync: error_code={ErrorCode}", result.error_code);
                        return Deserialize<SearchResults<Repository>>(result).Items;
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        // ===== Commits API =====

        public Task<List<Commit>> GetCommitsAsync(string owner, string repo, byte perPage = 30, uint page = 1)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_get_commits(wrapper, (sbyte*)pOwner, (sbyte*)pRepo, perPage, page);
                        _logger.LogDebug("GetCommitsAsync: error_code={ErrorCode}", result.error_code);
                        return DeserializeList<Commit>(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        // ===== Organization API =====

        public Task<Organization> GetOrganizationAsync(string org)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOrg = Encoding.UTF8.GetBytes(org + "\0"))
                    {
                        var result = Methods.gh_get_organization(wrapper, (sbyte*)pOrg);
                        _logger.LogDebug("GetOrganizationAsync: error_code={ErrorCode}", result.error_code);
                        return Deserialize<Organization>(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        // ===== Languages API =====

        public Task<Dictionary<string, long>> GetRepositoryLanguagesAsync(string owner, string repo)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        var result = Methods.gh_get_repo_languages(wrapper, (sbyte*)pOwner, (sbyte*)pRepo);
                        _logger.LogDebug("GetRepositoryLanguagesAsync: error_code={ErrorCode}", result.error_code);
                        var json = MarshalString(result);
                        return (Dictionary<string, long>)(JsonSerializer.Deserialize(json, SerializationContext.Default.GetTypeInfo(typeof(Dictionary<string, long>))!) ?? new Dictionary<string, long>());
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }
    // ===== Notifications API =====

        public Task<List<NotificationThread>> GetNotificationsAsync(bool all = false, bool participating = false, string? since = null, string? before = null, uint page = 1, byte perPage = 50)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    var allInt = all ? 1 : 0;
                    var participatingInt = participating ? 1 : 0;

                    if (!string.IsNullOrEmpty(since) && !string.IsNullOrEmpty(before))
                    {
                        fixed (byte* pSince = Encoding.UTF8.GetBytes(since + "\0"))
                        fixed (byte* pBefore = Encoding.UTF8.GetBytes(before + "\0"))
                        {
                            var result = Methods.gh_get_notifications(wrapper, allInt, participatingInt, (sbyte*)pSince, (sbyte*)pBefore, page, perPage);
                            return DeserializeList<NotificationThread>(result);
                        }
                    }
                    else if (!string.IsNullOrEmpty(since))
                    {
                        fixed (byte* pSince = Encoding.UTF8.GetBytes(since + "\0"))
                        {
                            var result = Methods.gh_get_notifications(wrapper, allInt, participatingInt, (sbyte*)pSince, null, page, perPage);
                            return DeserializeList<NotificationThread>(result);
                        }
                    }
                    else if (!string.IsNullOrEmpty(before))
                    {
                        fixed (byte* pBefore = Encoding.UTF8.GetBytes(before + "\0"))
                        {
                            var result = Methods.gh_get_notifications(wrapper, allInt, participatingInt, null, (sbyte*)pBefore, page, perPage);
                            return DeserializeList<NotificationThread>(result);
                        }
                    }
                    else
                    {
                        var result = Methods.gh_get_notifications(wrapper, allInt, participatingInt, null, null, page, perPage);
                        return DeserializeList<NotificationThread>(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task MarkNotificationsReadAsync(string? lastReadAt = null)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    if (!string.IsNullOrEmpty(lastReadAt))
                    {
                        fixed (byte* pLastReadAt = Encoding.UTF8.GetBytes(lastReadAt + "\0"))
                        {
                            var result = Methods.gh_mark_notifications_read(wrapper, (sbyte*)pLastReadAt);
                            MarshalString(result);
                        }
                    }
                    else
                    {
                        var result = Methods.gh_mark_notifications_read(wrapper, null);
                        MarshalString(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<NotificationThread> GetThreadAsync(string threadId)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pThreadId = Encoding.UTF8.GetBytes(threadId + "\0"))
                    {
                        var result = Methods.gh_get_thread(wrapper, (sbyte*)pThreadId);
                        return Deserialize<NotificationThread>(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task MarkThreadReadAsync(string threadId)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pThreadId = Encoding.UTF8.GetBytes(threadId + "\0"))
                    {
                        var result = Methods.gh_mark_thread_read(wrapper, (sbyte*)pThreadId);
                        MarshalString(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task MarkThreadDoneAsync(string threadId)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pThreadId = Encoding.UTF8.GetBytes(threadId + "\0"))
                    {
                        var result = Methods.gh_mark_thread_done(wrapper, (sbyte*)pThreadId);
                        MarshalString(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<ThreadSubscription> GetThreadSubscriptionAsync(string threadId)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pThreadId = Encoding.UTF8.GetBytes(threadId + "\0"))
                    {
                        var result = Methods.gh_get_thread_subscription(wrapper, (sbyte*)pThreadId);
                        return Deserialize<ThreadSubscription>(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<ThreadSubscription> SetThreadSubscriptionAsync(string threadId, bool ignored)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pThreadId = Encoding.UTF8.GetBytes(threadId + "\0"))
                    {
                        var result = Methods.gh_set_thread_subscription(wrapper, (sbyte*)pThreadId, ignored ? 1 : 0);
                        return Deserialize<ThreadSubscription>(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task DeleteThreadSubscriptionAsync(string threadId)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pThreadId = Encoding.UTF8.GetBytes(threadId + "\0"))
                    {
                        var result = Methods.gh_delete_thread_subscription(wrapper, (sbyte*)pThreadId);
                        MarshalString(result);
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task<List<NotificationThread>> GetRepositoryNotificationsAsync(string owner, string repo, bool all = false, bool participating = false, string? since = null, string? before = null, byte perPage = 30, uint page = 1)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    var allInt = all ? 1 : 0;
                    var participatingInt = participating ? 1 : 0;

                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        if (!string.IsNullOrEmpty(since) && !string.IsNullOrEmpty(before))
                        {
                            fixed (byte* pSince = Encoding.UTF8.GetBytes(since + "\0"))
                            fixed (byte* pBefore = Encoding.UTF8.GetBytes(before + "\0"))
                            {
                                var result = Methods.gh_get_repo_notifications(wrapper, (sbyte*)pOwner, (sbyte*)pRepo, allInt, participatingInt, (sbyte*)pSince, (sbyte*)pBefore, perPage, page);
                                return DeserializeList<NotificationThread>(result);
                            }
                        }
                        else if (!string.IsNullOrEmpty(since))
                        {
                            fixed (byte* pSince = Encoding.UTF8.GetBytes(since + "\0"))
                            {
                                var result = Methods.gh_get_repo_notifications(wrapper, (sbyte*)pOwner, (sbyte*)pRepo, allInt, participatingInt, (sbyte*)pSince, null, perPage, page);
                                return DeserializeList<NotificationThread>(result);
                            }
                        }
                        else if (!string.IsNullOrEmpty(before))
                        {
                            fixed (byte* pBefore = Encoding.UTF8.GetBytes(before + "\0"))
                            {
                                var result = Methods.gh_get_repo_notifications(wrapper, (sbyte*)pOwner, (sbyte*)pRepo, allInt, participatingInt, null, (sbyte*)pBefore, perPage, page);
                                return DeserializeList<NotificationThread>(result);
                            }
                        }
                        else
                        {
                            var result = Methods.gh_get_repo_notifications(wrapper, (sbyte*)pOwner, (sbyte*)pRepo, allInt, participatingInt, null, null, perPage, page);
                            return DeserializeList<NotificationThread>(result);
                        }
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }

        public Task MarkRepositoryNotificationsReadAsync(string owner, string repo, string? lastReadAt = null)
        {
            return Task.Run(() =>
            {
                var wrapper = CreateAndInit();
                try
                {
                    fixed (byte* pOwner = Encoding.UTF8.GetBytes(owner + "\0"))
                    fixed (byte* pRepo = Encoding.UTF8.GetBytes(repo + "\0"))
                    {
                        if (!string.IsNullOrEmpty(lastReadAt))
                        {
                            fixed (byte* pLastReadAt = Encoding.UTF8.GetBytes(lastReadAt + "\0"))
                            {
                                var result = Methods.gh_mark_repo_notifications_read(wrapper, (sbyte*)pOwner, (sbyte*)pRepo, (sbyte*)pLastReadAt);
                                MarshalString(result);
                            }
                        }
                        else
                        {
                            var result = Methods.gh_mark_repo_notifications_read(wrapper, (sbyte*)pOwner, (sbyte*)pRepo, null);
                            MarshalString(result);
                        }
                    }
                }
                finally
                {
                    Methods.gh_deinit(wrapper);
                }
            });
        }
    }
}