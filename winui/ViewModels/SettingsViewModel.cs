using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StarSorter.Helpers;
using StarSorter.Services;
using System;
using Windows.Storage;

namespace StarSorter.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ThemeSelectorService _themeSelectorService;

        public string AppVersion
        {
            get
            {
                try
                {
                    var ver = Windows.ApplicationModel.Package.Current.Id.Version;
                    return $"Version {ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";
                }
                catch
                {
                    return "Version 1.0.0.0";
                }
            }
        }
        private readonly GitHubService _gitHub;

        private string _selectedTheme = string.Empty;

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    OnSelectedThemeChanged(value);
                }
            }
        }

        partial void OnSelectedThemeChanged(string value);

        [ObservableProperty] public partial string GitHubToken { get; set; } = string.Empty;

        [ObservableProperty] public partial bool IsTokenSaved { get; set; }

        public string TokenStatus => IsTokenSaved ? GetLocalizedString("TokenSaved") : GetLocalizedString("TokenNotConfigured");

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProxyStatus))]
        [NotifyPropertyChangedFor(nameof(IsCustomProxy))]
        public partial string ProxyMode { get; set; } = "Disabled";

        [ObservableProperty] public partial string CustomProxyUrl { get; set; } = string.Empty;

        public bool IsCustomProxy => ProxyMode == nameof(Helpers.ProxyMode.Custom);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PaneDisplayModeStatus))]
        public partial string PaneDisplayMode { get; set; } = "Auto";

        [ObservableProperty]
        public partial string AppLanguage { get; set; } = "Default";

        public string PaneDisplayModeStatus => PaneDisplayMode == "Auto" ? GetLocalizedString("AutoMode") : GetLocalizedString("ManualMode");

        [ObservableProperty]
        public partial bool InfoBarOpen { get; set; }

        [ObservableProperty]
        public partial InfoBarSeverity InfoBarSeverity { get; set; }

        [ObservableProperty]
        public partial string InfoBarMessage { get; set; } = string.Empty;

        public void UpdatePaneDisplayMode(string mode)
        {
            PaneDisplayMode = mode;
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values[SettingsKeys.NavigationPaneDisplayMode] = mode;
        }

        public event Action? LanguageChanged;

        public void UpdateAppLanguage(string language)
        {
            var settings = ApplicationData.Current.LocalSettings;

            if (language == "Default" && !settings.Values.ContainsKey(SettingsKeys.ApplicationLanguage))
                return;

            if (settings.Values.TryGetValue(SettingsKeys.ApplicationLanguage, out var savedLang) && 
                savedLang is string savedLangStr && savedLangStr == language)
            {
                return;
            }

            AppLanguage = language;
            settings.Values[SettingsKeys.ApplicationLanguage] = language;

            try
            {
                if (language == "Default")
                {
                    // 清除语言覆盖，让应用跟随系统语言
                    Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "";
                }
                else
                {
                    var culture = new System.Globalization.CultureInfo(language);
                    System.Globalization.CultureInfo.CurrentUICulture = culture;
                    System.Globalization.CultureInfo.CurrentCulture = culture;
                    Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = language;
                }
            }
            catch
            {
            }

            LanguageChanged?.Invoke();
        }

        public string ProxyStatus
        {
            get
            {
                return ProxyMode switch
                {
                    nameof(Helpers.ProxyMode.Disabled) => GetLocalizedString("ProxyDisabled"),
                    nameof(Helpers.ProxyMode.System) => GetLocalizedString("UsingSystemProxy"),
                    nameof(Helpers.ProxyMode.Custom) => GetLocalizedString("UsingCustomProxy"),
                    _ => GetLocalizedString("ProxyDisabled")
                };
            }
        }

        public SettingsViewModel(ThemeSelectorService themeSelectorService, GitHubService gitHub)
        {
            _themeSelectorService = themeSelectorService;
            _gitHub = gitHub;
            _selectedTheme = _themeSelectorService.CurrentTheme.ToString();
            LoadSettings();
        }

        partial void OnSelectedThemeChanged(string value)
        {
            if (Enum.TryParse<ElementTheme>(value, out var theme))
            {
                _themeSelectorService.SetTheme(theme);
            }
        }

        private void ShowInfoBar(string message, InfoBarSeverity severity)
        {
            InfoBarOpen = false;
            InfoBarMessage = message;
            InfoBarSeverity = severity;
            InfoBarOpen = true;
        }

        public void SaveGitHubToken()
        {
            if (!string.IsNullOrWhiteSpace(GitHubToken))
            {
                _gitHub.SaveToken(GitHubToken);
                IsTokenSaved = true;
                OnPropertyChanged(nameof(TokenStatus));
                ShowInfoBar(GetLocalizedString("TokenSavedSuccessfully"), InfoBarSeverity.Success);
            }
            else
            {
                ShowInfoBar(GetLocalizedString("TokenCannotBeEmpty"), InfoBarSeverity.Error);
            }
        }

        public void LoadSettings()
        {
            GitHubToken = _gitHub.GetSavedToken() ?? string.Empty;
            IsTokenSaved = !string.IsNullOrEmpty(GitHubToken);
            ProxyMode = _gitHub.GetProxyMode().ToString();
            CustomProxyUrl = _gitHub.GetCustomProxyUrl() ?? string.Empty;
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.TryGetValue(SettingsKeys.NavigationPaneDisplayMode, out var saved))
            {
                PaneDisplayMode = saved as string ?? "Auto";
            }

            if (settings.Values.TryGetValue(SettingsKeys.ApplicationLanguage, out var lang))
            {
                AppLanguage = lang as string ?? "Default";
            }
        }

        public void UpdateProxyMode(string mode)
        {
            ProxyMode = mode;
            if (Enum.TryParse<ProxyMode>(ProxyMode, out var parsed))
            {
                _gitHub.SaveProxyMode(parsed);
            }

            OnPropertyChanged(nameof(ProxyStatus));
        }

        public void SaveProxySettings()
        {
            if (Enum.TryParse<ProxyMode>(ProxyMode, out var mode))
            {
                _gitHub.SaveProxyMode(mode);
            }

            if (mode == Helpers.ProxyMode.Custom)
            {
                _gitHub.SaveCustomProxyUrl(CustomProxyUrl);
            }

            OnPropertyChanged(nameof(ProxyStatus));
            ShowInfoBar(GetLocalizedString("ProxySettingsSaved"), InfoBarSeverity.Success);
        }

        public string GetLocalizedString(string key)
        {
            return LocalizationHelper.GetLocalizedString(key);
        }
    }
}