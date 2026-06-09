using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using StarSorter.Helpers;
using StarSorter.Services;
using Windows.Storage;

namespace StarSorter.ViewModels
{
    public partial class ShellViewModel(NavigationService navigationService) : ObservableObject
    {
        private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        public event EventHandler<(string Title, string Message)>? DialogRequested;

        public void RequestDialog(string title, string message)
        {
            _dispatcherQueue.TryEnqueue(() =>
                DialogRequested?.Invoke(this, (title, message)));
        }

        private bool _isBackEnabled;

        public bool IsBackEnabled
        {
            get => _isBackEnabled;
            set
            {
                if (SetProperty(ref _isBackEnabled, value))
                {
                    OnIsBackEnabledChanged();
                }
            }
        }

        partial void OnIsBackEnabledChanged();

        private object? _selectedNavItem;

        public object? SelectedNavItem
        {
            get => _selectedNavItem;
            set
            {
                if (SetProperty(ref _selectedNavItem, value))
                {
                    OnSelectedNavItemChanged();
                }
            }
        }

        partial void OnSelectedNavItemChanged();

        private NavigationViewPaneDisplayMode _paneDisplayMode;

        public NavigationViewPaneDisplayMode PaneDisplayMode
        {
            get => _paneDisplayMode;
            set
            {
                if (SetProperty(ref _paneDisplayMode, value))
                {
                    var settings = ApplicationData.Current.LocalSettings;
                    settings.Values[SettingsKeys.NavigationPaneDisplayMode] = value.ToString();
                }
            }
        }

        public void LoadPaneDisplayMode()
        {
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.TryGetValue(SettingsKeys.NavigationPaneDisplayMode, out var saved) && saved is string modeStr)
            {
                if (Enum.TryParse<NavigationViewPaneDisplayMode>(modeStr, out var mode))
                {
                    _paneDisplayMode = mode;
                    OnPropertyChanged(nameof(PaneDisplayMode));
                    return;
                }
            }
            _paneDisplayMode = NavigationViewPaneDisplayMode.Auto;
            OnPropertyChanged(nameof(PaneDisplayMode));
        }

        public List<NavigationViewItem> MenuItems =>
    [
        new() { Content = GetLocalizedString("Stars"), Tag = "Stars", Icon = new SymbolIcon(Symbol.Home) },
        new() { Content = GetLocalizedString("Profile"), Tag = "Profile", Icon = new SymbolIcon(Symbol.People) },
        new() { Content = GetLocalizedString("Explore"), Tag = "Explore", Icon = new SymbolIcon(Symbol.Globe) },
        new() { Content = GetLocalizedString("Notifications"), Tag = "Notifications", Icon = new SymbolIcon(Symbol.Character) },
    ];

        public List<NavigationViewItem> FooterMenuItems =>
    [
        new () { Content = GetLocalizedString("Settings"), Tag = "Settings", Icon = new SymbolIcon(Symbol.Setting) }
    ];

        public void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                IsBackEnabled = navigationService.CanGoBack;
            });
        }

        public string GetLocalizedString(string key)
        {
            return LocalizationHelper.GetLocalizedString(key);
        }
    }

}