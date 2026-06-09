using CommunityToolkit.Mvvm.ComponentModel;
using StarSorter.Helpers;
using StarSorter.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace StarSorter.ViewModels
{
    public partial class StarDetailViewModel : ObservableObject
    {
        private readonly GitHubService _gitHub;
        private readonly DataManagerService _dataManager;

        [ObservableProperty]
        public partial Repository Repository { get; set; } = new()
        {
            Owner = new GitHubUser(),
            License = new License(),
        };

        [ObservableProperty]
        public partial bool IsLoading { get; set; } = true;

        [ObservableProperty]
        public partial bool HasError { get; set; }

        [ObservableProperty]
        public partial bool IsContentVisible { get; set; }

        partial void OnIsLoadingChanged(bool value) => IsContentVisible = !(value || HasError);
        partial void OnHasErrorChanged(bool value) => IsContentVisible = !(IsLoading || value);

        [ObservableProperty]
        public partial string? ErrorMessage { get; set; }

        [ObservableProperty]
        public partial string? ReadmeContent { get; set; }

        [ObservableProperty]
        public partial bool HasReadme { get; set; }

        [ObservableProperty]
        public partial bool HasNoReadme { get; set; } = true;

        [ObservableProperty]
        public partial bool HasReleases { get; set; }

        [ObservableProperty]
        public partial bool HasNoReleases { get; set; } = true;

        [ObservableProperty]
        public partial ObservableCollection<Release> Releases { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<Contributor> Contributors { get; set; } = new();

        public Release LatestRelease => Releases.Count > 0 ? Releases[0] : new();

        partial void OnReleasesChanged(ObservableCollection<Release> value) => OnPropertyChanged(nameof(LatestRelease));

        public StarDetailViewModel(GitHubService gitHub, DataManagerService dataManager)
        {
            _gitHub = gitHub;
            _dataManager = dataManager;
        }

        public async Task LoadDataAsync(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return;

            IsLoading = true;
            HasError = false;
            ErrorMessage = null;

            try
            {
                var parts = fullName.Split('/');
                if (parts.Length != 2)
                {
                    HasError = true;
                    ErrorMessage = LocalizationHelper.GetLocalizedString("InvalidRepoName");
                    return;
                }

                var owner = parts[0];
                var repo = parts[1];
                var cacheKey = $"repo_detail_{fullName}";

                // 尝试从缓存加载（仅当有有效数据时使用）
                var cached = await _dataManager.LoadApiCacheAsync(cacheKey);
                if (cached != null)
                {
                    var cachedData = JsonSerializer.Deserialize(cached, SerializationContext.CamelCase.CachedRepoDetail);
                    if (cachedData?.Repository?.Id > 0)
                    {
                        Repository = cachedData.Repository;
                        Contributors = new ObservableCollection<Contributor>(cachedData.Contributors);
                        Releases = new ObservableCollection<Release>(cachedData.Releases);
                        ReadmeContent = cachedData.Readme;
                        HasReadme = !string.IsNullOrEmpty(cachedData.Readme);
                        HasNoReadme = string.IsNullOrEmpty(cachedData.Readme);
                        HasReleases = cachedData.Releases.Count > 0;
                        HasNoReleases = cachedData.Releases.Count == 0;
                        IsLoading = false;
                        return;
                    }
                }

                // 并发获取仓库信息、贡献者、发行版和 README
                var repoTask = _gitHub.GetRepositoryAsync(owner, repo);
                var contributorsTask = _gitHub.GetContributorsAsync(owner, repo);
                var releasesTask = _gitHub.GetReleasesAsync(owner, repo);

                // README 可能不存在，单独处理异常
                var readmeTask = GetReadmeSafeAsync(owner, repo);

                await Task.WhenAll(repoTask, contributorsTask, releasesTask, readmeTask);

                Repository = await repoTask;
                Contributors = new ObservableCollection<Contributor>(await contributorsTask);
                var releases = await releasesTask;
                Releases = new ObservableCollection<Release>(releases);
                ReadmeContent = await readmeTask;
                HasReadme = !string.IsNullOrEmpty(ReadmeContent);
                HasNoReadme = string.IsNullOrEmpty(ReadmeContent);
                HasReleases = releases.Count > 0;
                HasNoReleases = releases.Count == 0;

                // 保存到缓存
                var cacheData = new CachedRepoDetail
                {
                    Repository = Repository,
                    Contributors = Contributors.ToList(),
                    Releases = Releases.ToList(),
                    Readme = ReadmeContent,
                    CachedAt = DateTime.UtcNow
                };
                var cacheJson = JsonSerializer.Serialize(cacheData, SerializationContext.CamelCase.CachedRepoDetail);
                await _dataManager.SaveApiCacheAsync(cacheKey, cacheJson);
            }
            catch (GitHubApiException ex)
            {
                HasError = true;
                ErrorMessage = LocalizationHelper.GetLocalizedString("ErrorLoadRepoDetails", ex.Message);
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = LocalizationHelper.GetLocalizedString("ErrorLoadRepoDetails", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<string?> GetReadmeSafeAsync(string owner, string repo)
        {
            try
            {
                return await _gitHub.GetRepositoryReadmeAsync(owner, repo);
            }
            catch
            {
                return null;
            }
        }

        public string GetLocalizedString(string key) => LocalizationHelper.GetLocalizedString(key);
    }
}