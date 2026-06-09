using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StarSorter.Helpers;
using StarSorter.ViewModels;
using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.System;

namespace StarSorter.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<SettingsViewModel>();
            ViewModel.LanguageChanged += ViewModel_LanguageChanged;
        }

        private async void ViewModel_LanguageChanged()
        {
            var dialog = UIHelper.CreateThemedDialog(this.XamlRoot);
            dialog.Title = LocalizationHelper.GetLocalizedString("RestartRequired");
            dialog.Content = LocalizationHelper.GetLocalizedString("LanguageChangedRestart");
            dialog.PrimaryButtonText = LocalizationHelper.GetLocalizedString("RestartNow");
            dialog.SecondaryButtonText = LocalizationHelper.GetLocalizedString("RestartLater");

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule?.FileName,
                    UseShellExecute = true
                });
                Application.Current.Exit();
            }
        }

        private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
        {
            var currentTheme = ThemeHelper.RootTheme;
            switch (currentTheme)
            {
                case ElementTheme.Light:
                    themeMode.SelectedIndex = 0;
                    break;
                case ElementTheme.Dark:
                    themeMode.SelectedIndex = 1;
                    break;
                case ElementTheme.Default:
                    themeMode.SelectedIndex = 2;
                    break;
            }

            var currentBackground = ThemeHelper.WindowBackground;
            switch (currentBackground)
            {
                case WindowBackgroundType.MicaBase:
                    windowBackground.SelectedIndex = 0;
                    break;
                case WindowBackgroundType.MicaAlt:
                    windowBackground.SelectedIndex = 1;
                    break;
                case WindowBackgroundType.AcrylicBase:
                    windowBackground.SelectedIndex = 2;
                    break;
                case WindowBackgroundType.AcrylicThin:
                    windowBackground.SelectedIndex = 3;
                    break;
                case WindowBackgroundType.None:
                    windowBackground.SelectedIndex = 4;
                    break;
            }

            var currentProxy = ViewModel.ProxyMode;
            switch (currentProxy)
            {
                case "Disabled":
                    proxyModeCombo.SelectedIndex = 0;
                    break;
                case "System":
                    proxyModeCombo.SelectedIndex = 1;
                    break;
                case "Custom":
                    proxyModeCombo.SelectedIndex = 2;
                    break;
            }

            var navMode = ViewModel.PaneDisplayMode;
            for (int i = 0; i < navPaneModeCombo.Items.Count; i++)
            {
                if (navPaneModeCombo.Items[i] is ComboBoxItem item && item.Tag?.ToString() == navMode)
                {
                    navPaneModeCombo.SelectedIndex = i;
                    break;
                }
            }

            var lang = ViewModel.AppLanguage;
            switch (lang)
            {
                case "Default":
                    appLanguageCombo.SelectedIndex = 0;
                    break;
                case "zh-CN":
                    appLanguageCombo.SelectedIndex = 1;
                    break;
                case "en-US":
                    appLanguageCombo.SelectedIndex = 2;
                    break;
            }

            var settings = ApplicationData.Current.LocalSettings;
            var savedBg = settings.Values[SettingsKeys.MarkdownBackgroundColor] as string;
            if (!string.IsNullOrEmpty(savedBg))
            {
                for (int i = 0; i < markdownBgColor.Items.Count; i++)
                {
                    if (markdownBgColor.Items[i] is ComboBoxItem item && item.Tag?.ToString() == savedBg)
                    {
                        markdownBgColor.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                markdownBgColor.SelectedIndex = 0;
            }
        }

        private void ThemeMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTheme = ((ComboBoxItem)themeMode.SelectedItem)?.Tag?.ToString();
            var window = WindowHelper.GetWindowForElement(this);
            if (selectedTheme != null)
            {
                ThemeHelper.RootTheme = EnumHelper.GetEnum<ElementTheme>(selectedTheme);
                var elementThemeResolved = ThemeHelper.RootTheme == ElementTheme.Default
                    ? ThemeHelper.ActualTheme
                    : ThemeHelper.RootTheme;
                TitleBarHelper.ApplySystemThemeToCaptionButtons(window, elementThemeResolved);
            }
        }

        private void WindowBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedBackground = ((ComboBoxItem)windowBackground.SelectedItem)?.Tag?.ToString();
            if (selectedBackground != null)
            {
                ThemeHelper.WindowBackground = EnumHelper.GetEnum<WindowBackgroundType>(selectedBackground);
            }
        }

        private void SaveToken_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveGitHubToken();
        }

        private void ProxyMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ((ComboBoxItem)proxyModeCombo.SelectedItem)?.Tag?.ToString();
            if (selected != null)
            {
                ViewModel.UpdateProxyMode(selected);
            }
        }

        private void SaveProxy_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveProxySettings();
        }

        private void NavPaneMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ((ComboBoxItem)navPaneModeCombo.SelectedItem)?.Tag?.ToString();
            if (selected != null)
            {
                ViewModel.UpdatePaneDisplayMode(selected);
                var shellVm = App.Current.Services.GetRequiredService<ShellViewModel>();
                if (Enum.TryParse<NavigationViewPaneDisplayMode>(selected, out var mode))
                {
                    shellVm.PaneDisplayMode = mode;
                }
            }
        }

        private void AppLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ((ComboBoxItem)appLanguageCombo.SelectedItem)?.Tag?.ToString();
            if (selected != null)
            {
                ViewModel.UpdateAppLanguage(selected);
            }
        }

        private void MarkdownBgColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ((ComboBoxItem)markdownBgColor.SelectedItem)?.Tag?.ToString();
            if (!string.IsNullOrEmpty(selected))
            {
                ApplicationData.Current.LocalSettings.Values[SettingsKeys.MarkdownBackgroundColor] = selected;
            }
        }

        private async void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://github.com/WWW4R4E/StarSorter/releases"));
        }
    }
}