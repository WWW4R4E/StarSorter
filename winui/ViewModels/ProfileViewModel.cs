using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using StarSorter.Helpers;
using StarSorter.Native;
using StarSorter.Services;
using System;

namespace StarSorter.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly GitHubService _gitHub;
        private readonly DataManagerService _dataManager;
        private readonly ShellViewModel _shellVm;
        private bool _hasLoaded;

        [ObservableProperty]
        public partial GitHubUser User { get; set; } = new();

        [ObservableProperty]
        public partial string RateLimitCore { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string RateLimitSearch { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string LabelCoreApiDescription { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string LabelSearchApiDescription { get; set; } = string.Empty;

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

        public Visibility BioVisibility => !string.IsNullOrEmpty(User.Bio) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CompanyVisibility => !string.IsNullOrEmpty(User.Company) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LocationVisibility => !string.IsNullOrEmpty(User.Location) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EmailVisibility => !string.IsNullOrEmpty(User.Email) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility BlogVisibility => !string.IsNullOrEmpty(User.Blog) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility TwitterVisibility => !string.IsNullOrEmpty(User.TwitterUsername) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NameVisibility => !string.IsNullOrEmpty(User.Name) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LoginOnlyVisibility => string.IsNullOrEmpty(User.Name) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PlanVisibility => User.Plan != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DetailsVisibility =>
            !string.IsNullOrEmpty(User.Company) ||
            !string.IsNullOrEmpty(User.Location) ||
            !string.IsNullOrEmpty(User.Email) ||
            !string.IsNullOrEmpty(User.TwitterUsername) ? Visibility.Visible : Visibility.Collapsed;

        public ProfileViewModel(GitHubService gitHub, DataManagerService dataManager, ShellViewModel shellVm)
        {
            _gitHub = gitHub;
            _dataManager = dataManager;
            _shellVm = shellVm;
            _ = LoadUserProfileAsync();
        }

        partial void OnUserChanged(GitHubUser value)
        {
            OnPropertyChanged(nameof(BioVisibility));
            OnPropertyChanged(nameof(CompanyVisibility));
            OnPropertyChanged(nameof(LocationVisibility));
            OnPropertyChanged(nameof(EmailVisibility));
            OnPropertyChanged(nameof(BlogVisibility));
            OnPropertyChanged(nameof(TwitterVisibility));
            OnPropertyChanged(nameof(NameVisibility));
            OnPropertyChanged(nameof(LoginOnlyVisibility));
            OnPropertyChanged(nameof(PlanVisibility));
            OnPropertyChanged(nameof(DetailsVisibility));
        }

        public async Task LoadUserProfileAsync()
        {
            if (_hasLoaded) return;
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var cached = await _dataManager.LoadUserProfileAsync<GitHubUser>();
                if (cached != null)
                {
                    User = cached;
                }
                else
                {
                    var user = await _gitHub.GetCurrentUserAsync();
                    User = user;
                    await _dataManager.SaveUserProfileAsync(user);
                }

                try
                {
                    var rateLimit = await _gitHub.CheckRateLimitAsync();
                    var core = rateLimit.Resources.Core;
                    var search = rateLimit.Resources.Search;
                    RateLimitCore = $"{core.Remaining:N0} / {core.Limit:N0}";
                    LabelCoreApiDescription = $"{core.Used:N0} used";
                    RateLimitSearch = $"{search.Remaining:N0} / {search.Limit:N0}";
                    LabelSearchApiDescription = $"{search.Used:N0} used";
                }
                catch
                {
                    RateLimitCore = "N/A";
                    RateLimitSearch = "N/A";
                }

                _hasLoaded = true;
            }
            catch (GitHubApiException ex)
            {
                ErrorMessage = LocalizationHelper.GetLocalizedString("ErrorLoadProfile", ex.Message);
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
                ErrorMessage = LocalizationHelper.GetLocalizedString("ErrorLoadProfile", ex.Message);
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void LoadUserProfile()
        {
            _hasLoaded = false;
            var filePath = Path.Combine(
                Microsoft.Windows.Storage.ApplicationData.GetDefault().LocalFolder.Path,
                "data", "user_profile.json");
            if (File.Exists(filePath))
                File.Delete(filePath);
            _ = LoadUserProfileAsync();
        }

        public string GetLocalizedString(string key) => LocalizationHelper.GetLocalizedString(key);
    }
}
