using CommunityToolkit.Mvvm.ComponentModel;
using StarSorter.Helpers;
using StarSorter.Native;
using StarSorter.Services;
using System.Collections.ObjectModel;

namespace StarSorter.ViewModels
{
    public partial class StarsViewModel : ObservableObject
    {
        private readonly GitHubService _gitHub;
        private readonly DataManagerService _dataManager;
        private readonly ShellViewModel _shellVm;
        private bool _hasLoaded;

        private ObservableCollection<StarRepository> _allStars = new();

        public ObservableCollection<StarRepository> AllStars => _allStars;

        [ObservableProperty]
        public partial ObservableCollection<StarRepository> CurrentCategoryStars { get; set; } = new();

        public ObservableCollection<NavLink> NavLinks { get; set; } = new();

        [ObservableProperty] public partial NavLink? SelectedCategory { get; set; }

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
        public partial string NewCategoryName { get; set; } = string.Empty;

        public StarsViewModel(GitHubService gitHub, DataManagerService dataManager, ShellViewModel shellVm)
        {
            _gitHub = gitHub;
            _dataManager = dataManager;
            _shellVm = shellVm;
            EnsureBuiltInNavLinks();
            _ = LoadStarsAsync();
        }

        private string AllStarsLabel => LocalizationHelper.GetLocalizedString("AllStars");
        private string UncategorizedLabel => LocalizationHelper.GetLocalizedString("Uncategorized");

        private void EnsureBuiltInNavLinks()
        {
            var allStars = AllStarsLabel;
            var existingAllStars = NavLinks.FirstOrDefault(n => n.IsBuiltIn && n.Glyph == "\uE734");
            if (existingAllStars != null)
            {
                existingAllStars.Label = allStars;
            }
            else
            {
                NavLinks.Insert(0, new NavLink { Label = allStars, Glyph = "\uE734", IsBuiltIn = true });
            }
        }

        private void EnsureUncategorizedLink()
        {
            var uncategorized = UncategorizedLabel;
            var existing = NavLinks.FirstOrDefault(n => n.IsBuiltIn && n.Glyph == "\uE784");
            if (existing != null)
            {
                existing.Label = uncategorized;
            }
        }

        public async Task LoadStarsAsync()
        {
            if (_hasLoaded) return;
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var cached = await _dataManager.LoadStarsDataAsync();
                if (cached.Stars.Count > 0)
                {
                    _allStars = new ObservableCollection<StarRepository>(cached.Stars);
                    NavLinks.Clear();
                    foreach (var link in cached.Categories)
                        NavLinks.Add(link);
                    EnsureBuiltInNavLinks();
                    EnsureUncategorizedLink();
                    UpdateUncategorizedLink();
                    _hasLoaded = true;
                    LoadStarsForCategory(SelectedCategory?.Label ?? AllStarsLabel);
                    return;
                }

                var repos = await _gitHub.GetAllStarredReposAsync();
                _allStars = new ObservableCollection<StarRepository>(
                    repos.Select(StarRepository.FromRepository));

                await _dataManager.SaveStarsDataAsync(_allStars.ToList(), NavLinks.ToList());
                _hasLoaded = true;
            }
            catch (GitHubApiException ex)
            {
                ErrorMessage = LocalizationHelper.GetLocalizedString("ErrorLoadStars", ex.Message);
                HasError = true;
                var title = ex.ErrorCode switch
                {
                    GhError.GH_FORBIDDEN => LocalizationHelper.GetLocalizedString("PermissionDenied"),
                    GhError.GH_UNAUTHORIZED => LocalizationHelper.GetLocalizedString("AuthFailed"),
                    _ => LocalizationHelper.GetLocalizedString("RequestFailed")
                };
                _shellVm.RequestDialog(title, ex.Message);
            }
            catch (Exception ex)
            {
                ErrorMessage = LocalizationHelper.GetLocalizedString("ErrorLoadStars", ex.Message);
                HasError = true;
            }
            finally
            {
                IsLoading = false;
                LoadStarsForCategory(SelectedCategory?.Label ?? AllStarsLabel);
            }
        }

        public void LoadStarsForCategory(string categoryName)
        {
            if (_allStars.Count == 0) return;

            var allStars = AllStarsLabel;
            var uncategorized = UncategorizedLabel;

            CurrentCategoryStars = categoryName switch
            {
                var name when name == allStars => new ObservableCollection<StarRepository>(_allStars),
                var name when name == uncategorized => new ObservableCollection<StarRepository>(
                    _allStars.Where(s => string.IsNullOrEmpty(s.Category))),
                _ => new ObservableCollection<StarRepository>(_allStars.Where(s => s.Category == categoryName))
            };
        }

        public void UpdateUncategorizedLink()
        {
            var hasUncategorized = _allStars.Any(s => string.IsNullOrEmpty(s.Category));
            var uncategorized = UncategorizedLabel;
            var existing = NavLinks.FirstOrDefault(n => n.Label == uncategorized);

            if (hasUncategorized && existing == null)
            {
                NavLinks.Insert(1, new NavLink { Label = uncategorized, Glyph = "\uE784", IsBuiltIn = true });
            }
            else if (!hasUncategorized && existing != null)
            {
                NavLinks.Remove(existing);
            }
        }

        public async System.Threading.Tasks.Task AddNewCategory()
        {
            if (!string.IsNullOrWhiteSpace(NewCategoryName))
            {
                var existingCategory = NavLinks.FirstOrDefault(n =>
                    !n.IsBuiltIn && n.Label.Equals(NewCategoryName, StringComparison.OrdinalIgnoreCase));
                if (existingCategory == null)
                {
                    NavLinks.Add(new NavLink { Label = NewCategoryName, Glyph = "\uE734" });
                    NewCategoryName = string.Empty;
                    UpdateUncategorizedLink();
                    await SaveCategoriesAsync(_allStars.ToList(), NavLinks.ToList());
                }
            }
        }

        public async System.Threading.Tasks.Task DeleteCategory(string label)
        {
            var category = NavLinks.FirstOrDefault(n => n.Label == label);
            if (category != null && !category.IsBuiltIn)
            {
                NavLinks.Remove(category);
                foreach (var star in _allStars.Where(s => s.Category == label))
                    star.Category = null;
                UpdateUncategorizedLink();
                if (SelectedCategory?.Label == label)
                    LoadStarsForCategory(AllStarsLabel);
                await SaveCategoriesAsync(_allStars.ToList(), NavLinks.ToList());
            }
        }

        public async System.Threading.Tasks.Task SaveCategoriesAsync(List<StarRepository> stars, List<NavLink> navLinks)
        {
            await _dataManager.SaveStarsDataAsync(stars, navLinks);
        }

        public void MoveStarToCategory(StarRepository star, string targetCategoryName)
        {
            star.Category = targetCategoryName;
        }

        public void RemoveStarFromCategory(StarRepository star)
        {
            star.Category = null;
        }

        public async System.Threading.Tasks.Task<bool> UnstarRepositoriesAsync(List<StarRepository> repos)
        {
            var successCount = 0;
            var failedRepos = new List<string>();

            foreach (var repo in repos)
            {
                if (string.IsNullOrEmpty(repo.OwnerName) || string.IsNullOrEmpty(repo.Name))
                {
                    failedRepos.Add(repo.Name);
                    continue;
                }

                try
                {
                    await _gitHub.UnstarRepositoryAsync(repo.OwnerName, repo.Name);
                    _allStars.Remove(repo);
                    successCount++;
                }
                catch (Exception)
                {
                    failedRepos.Add(repo.Name);
                }
            }

            if (successCount > 0)
            {
                UpdateUncategorizedLink();
                LoadStarsForCategory(SelectedCategory?.Label ?? AllStarsLabel);
                await _dataManager.SaveStarsDataAsync(_allStars.ToList(), NavLinks.ToList());
            }

            return failedRepos.Count == 0;
        }

        public string GetLocalizedString(string key) => LocalizationHelper.GetLocalizedString(key);

        public async System.Threading.Tasks.Task ReloadAsync()
        {
            _hasLoaded = false;
            _allStars.Clear();
            await _dataManager.ClearAllDataAsync();
            await LoadStarsAsync();
        }
    }
}
