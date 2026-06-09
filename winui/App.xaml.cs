using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using StarSorter.Helpers;
using StarSorter.Services;
using StarSorter.ViewModels;
using StarSorter.Views;
using System.Diagnostics;
using System.Globalization;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StarSorter
{
	public partial class App : Application
	{
		public ServiceProvider Services { get; }

		public static new App Current => (App)Application.Current;

		public static UIElement? AppTitlebar { get; set; }

		public App()
		{
			this.InitializeComponent();
			Services = ConfigureServices();
			this.UnhandledException += OnUnhandledException;
		}

		private void OnUnhandledException(object sender,
			Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
		{
			Debug.WriteLine($"未处理的异常: {e.Exception}");
		}

		protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
		{
			ApplySavedLanguage();
			ThemeHelper.Initialize();
			m_window = new MainWindow();
			Current.Services.GetRequiredService<ThemeSelectorService>().Initialize(m_window);
			m_window.Activate();
			WindowHelper.TrackWindow(m_window);
			ThemeHelper.EnsureBackgroundApplied();
		}

		private static void ApplySavedLanguage()
		{
			var settings = ApplicationData.Current.LocalSettings;
			if (settings.Values.TryGetValue(SettingsKeys.ApplicationLanguage, out var lang) && lang is string langStr &&
			    !string.IsNullOrEmpty(langStr) && langStr != "Default")
			{
				try
				{
					var culture = new CultureInfo(langStr);
					CultureInfo.CurrentUICulture = culture;
					CultureInfo.CurrentCulture = culture;
					Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = langStr;
				}
				catch
				{
				}
			}
		}


		private Window m_window = null!;

		private static ServiceProvider ConfigureServices()
		{
			var services = new ServiceCollection();

			// Services
			// 注册为单例，整个App共享一个实例
			services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));

			services.AddSingleton<NavigationService>(_ =>
			{
				var navigationService = new NavigationService();

				navigationService.Configure("Stars", typeof(StarsPage));
				navigationService.Configure("Profile", typeof(ProfilePage));
				navigationService.Configure("StarDetail", typeof(StarDetailPage));
				navigationService.Configure("Settings", typeof(SettingsPage));
				// TODO: 暂时隐藏 - ExplorePage 尚未完善
				//navigationService.Configure("Explore", typeof(ExplorePage));
				navigationService.Configure("Notifications", typeof(NotificationsPage));

				return navigationService;
			});
			services.AddSingleton<ThemeSelectorService>();
			services.AddSingleton<ImageCacheService>();
			services.AddSingleton<DataManagerService>();
			services.AddSingleton<CacheManager>();
			services.AddSingleton<GitHubService>();

			services.AddSingleton<ShellViewModel>();
			services.AddSingleton<StarsViewModel>();
			services.AddTransient<ProfileViewModel>();
			services.AddTransient<StarDetailViewModel>();
			services.AddTransient<SettingsViewModel>();
			// TODO: 暂时隐藏 - ExplorePage 尚未完善
			//services.AddTransient<ExploreViewModel>();
			services.AddSingleton<NotificationsViewModel>();

			return services.BuildServiceProvider();
		}
	}
}