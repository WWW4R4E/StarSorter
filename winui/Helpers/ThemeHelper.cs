using Microsoft.UI.Xaml;
using Microsoft.Windows.Storage;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using WinRT;

namespace StarSorter.Helpers
{
    public enum WindowBackgroundType
    {
        MicaBase,
        MicaAlt,
        AcrylicBase,
        AcrylicThin,
        None
    }

    public static partial class ThemeHelper
    {
        private static ApplicationData appData = ApplicationData.GetDefault();
        private static MicaController? micaController;
        private static DesktopAcrylicController? acrylicController;
        private static SystemBackdropConfiguration? backdropConfiguration;

        /// <summary>
        /// Gets the current actual theme of the app based on the requested theme of the
        /// root element, or if that value is Default, the requested theme of the Application.
        /// </summary>
        public static ElementTheme ActualTheme
        {
            get
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    if (window.Content is FrameworkElement rootElement)
                    {
                        if (rootElement.RequestedTheme != ElementTheme.Default)
                        {
                            return rootElement.RequestedTheme;
                        }
                    }
                }

                return EnumHelper.GetEnum<ElementTheme>(App.Current.RequestedTheme.ToString()!);
            }
        }

        /// <summary>
        /// Gets or sets (with LocalSettings persistence) the RequestedTheme of the root element.
        /// </summary>
        public static ElementTheme RootTheme
        {
            get
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    if (window.Content is FrameworkElement rootElement)
                    {
                        return rootElement.RequestedTheme;
                    }
                }

                return ElementTheme.Default;
            }
            set
            {
                if (value == RootTheme)
                {
                    return;
                }
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    if (window.Content is FrameworkElement rootElement)
                    {
                        rootElement.RequestedTheme = value;
                    }
                }

                appData.LocalSettings.Values[SettingsKeys.SelectedAppTheme] = value.ToString();

                if (backdropConfiguration != null)
                {
                    backdropConfiguration.Theme = value == ElementTheme.Default
                        ? SystemBackdropTheme.Default
                        : value == ElementTheme.Dark ? SystemBackdropTheme.Dark : SystemBackdropTheme.Light;
                }
            }
        }

        /// <summary>
        /// Gets or sets (with LocalSettings persistence) the window background type.
        /// </summary>
        public static WindowBackgroundType WindowBackground
        {
            get
            {
                string? savedBackground = appData.LocalSettings.Values[SettingsKeys.WindowBackgroundType]?.ToString();
                if (savedBackground != null)
                {
                    return EnumHelper.GetEnum<WindowBackgroundType>(savedBackground);
                }
                return WindowBackgroundType.MicaBase;
            }
            set
            {
                if (value == WindowBackground)
                {
                    return;
                }
                appData.LocalSettings.Values[SettingsKeys.WindowBackgroundType] = value.ToString();
                ApplyWindowBackground(value);
            }
        }

        public static void Initialize()
        {
            string? savedTheme = appData.LocalSettings.Values[SettingsKeys.SelectedAppTheme]?.ToString();

            if (savedTheme != null)
            {
                RootTheme = EnumHelper.GetEnum<ElementTheme>(savedTheme);
            }

            // Don't apply background immediately on startup, wait for windows to be ready
            // We'll apply the background when windows are available
        }

        public static bool IsDarkTheme()
        {
            if (RootTheme == ElementTheme.Default)
            {
                return Application.Current.RequestedTheme == ApplicationTheme.Dark;
            }
            return RootTheme == ElementTheme.Dark;
        }

        public static void EnsureBackgroundApplied()
        {
            // Apply the saved background to all active windows
            string? savedBackground = appData.LocalSettings.Values[SettingsKeys.WindowBackgroundType]?.ToString();
            if (savedBackground != null)
            {
                ApplyWindowBackground(EnumHelper.GetEnum<WindowBackgroundType>(savedBackground));
            }
            else
            {
                ApplyWindowBackground(WindowBackgroundType.MicaBase);
            }
        }

        public static void Cleanup()
        {
            // Clean up controllers when the app is closing
            micaController?.Dispose();
            acrylicController?.Dispose();
            micaController = null;
            acrylicController = null;
            backdropConfiguration = null;
        }

        private static SystemBackdropTheme GetBackdropTheme()
        {
            var resolvedTheme = RootTheme == ElementTheme.Default ? ActualTheme : RootTheme;
            return resolvedTheme == ElementTheme.Dark ? SystemBackdropTheme.Dark : SystemBackdropTheme.Light;
        }

        private static void ApplyWindowBackground(WindowBackgroundType backgroundType)
        {
            foreach (Window window in WindowHelper.ActiveWindows)
            {
                try
                {
                    if (window == null || window.Content == null)
                    {
                        continue;
                    }

                    // Dispose of existing controllers
                    micaController?.Dispose();
                    acrylicController?.Dispose();
                    micaController = null;
                    acrylicController = null;
                    backdropConfiguration = null;

                    backdropConfiguration = new SystemBackdropConfiguration
                    {
                        Theme = GetBackdropTheme()
                    };

                    switch (backgroundType)
                    {
                        case WindowBackgroundType.MicaBase:
                            if (MicaController.IsSupported())
                            {
                                micaController = new MicaController();
                                micaController.Kind = MicaKind.Base;

                                var backdropTarget = window.As<ICompositionSupportsSystemBackdrop>();
                                if (backdropTarget != null)
                                {
                                    micaController.AddSystemBackdropTarget(backdropTarget);
                                    micaController.SetSystemBackdropConfiguration(backdropConfiguration);
                                }
                            }
                            break;
                        case WindowBackgroundType.MicaAlt:
                            if (MicaController.IsSupported())
                            {
                                micaController = new MicaController();
                                micaController.Kind = MicaKind.BaseAlt;

                                var backdropTarget = window.As<ICompositionSupportsSystemBackdrop>();
                                if (backdropTarget != null)
                                {
                                    micaController.AddSystemBackdropTarget(backdropTarget);
                                    micaController.SetSystemBackdropConfiguration(backdropConfiguration);
                                }
                            }
                            break;
                        case WindowBackgroundType.AcrylicBase:
                            if (DesktopAcrylicController.IsSupported())
                            {
                                acrylicController = new DesktopAcrylicController
                                {
                                    Kind = DesktopAcrylicKind.Base
                                };

                                var backdropTarget = window.As<ICompositionSupportsSystemBackdrop>();
                                if (backdropTarget != null)
                                {
                                    acrylicController.AddSystemBackdropTarget(backdropTarget);
                                    acrylicController.SetSystemBackdropConfiguration(backdropConfiguration);
                                }
                            }
                            break;
                        case WindowBackgroundType.AcrylicThin:
                            if (DesktopAcrylicController.IsSupported())
                            {
                                acrylicController = new DesktopAcrylicController();
                                acrylicController.Kind = DesktopAcrylicKind.Thin;

                                var backdropTarget = window.As<ICompositionSupportsSystemBackdrop>();
                                if (backdropTarget != null)
                                {
                                    acrylicController.AddSystemBackdropTarget(backdropTarget);
                                    acrylicController.SetSystemBackdropConfiguration(backdropConfiguration);
                                }
                            }
                            break;
                        case WindowBackgroundType.None:
                            break;
                    }
                }
                catch
                {
                    // Silently handle exceptions when setting backdrop
                }
            }
        }
    }
}