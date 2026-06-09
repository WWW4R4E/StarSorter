using MarkWin2D.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using StarSorter.Helpers;
using StarSorter.Services;
using StarSorter.ViewModels;
using System.ComponentModel;

namespace StarSorter.Views
{
	public sealed partial class StarDetailPage : Page
	{
		public StarDetailViewModel ViewModel { get; }

		public StarDetailPage()
		{
			this.InitializeComponent();
			ViewModel = App.Current.Services.GetRequiredService<StarDetailViewModel>();
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			string fullName;
			if (e.Parameter is StarRepository repo)
			{
				fullName = repo.FullName;
			}
			else if (e.Parameter is string name)
			{
				fullName = name;
			}
			else
			{
				return;
			}

			// 设置 README Markdown 的图片加载器
			var cacheService = App.Current.Services.GetRequiredService<ImageCacheService>();
			ReadmeMarkdown.ImageProvider = new ReadmeImageProvider(cacheService, fullName);

			await ViewModel.LoadDataAsync(fullName);
		}

		public static Visibility ToVisibility(bool value)
		{
			return value ? Visibility.Visible : Visibility.Collapsed;
		}

		public static Visibility ToInvertedVisibility(bool value)
		{
			return value ? Visibility.Collapsed : Visibility.Visible;
		}

		public static Visibility ToVisibilityFromObject(object? obj)
		{
			return obj != null ? Visibility.Visible : Visibility.Collapsed;
		}

		public static string GetExpandButtonText(bool isExpanded)
		{
			return isExpanded ? "Show less" : "Show more";
		}

		private void OnToggleReleaseBody(object sender, RoutedEventArgs e)
		{
			if (sender is Button button && button.DataContext is Release release)
			{
				release.IsExpanded = !release.IsExpanded;
			}
		}

		//private void OnToggleReleases(object sender, RoutedEventArgs e)
		//{
		//    var isCollapsed = ReleasesItems.Visibility == Visibility.Collapsed;
		//    ReleasesItems.Visibility = isCollapsed ? Visibility.Visible : Visibility.Collapsed;
		//    ReleasesChevron.Glyph = isCollapsed ? "\uE70E" : "\uE70D";
		//}

		private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ViewModel.ReadmeContent))
			{
				if (ReadmeMarkdown != null)
				{
					ReadmeMarkdown.Text = ViewModel.ReadmeContent ?? string.Empty;
				}
			}
		}

		private void OnReleaseMarkdownLoaded(object sender, RoutedEventArgs e)
		{
			if (sender is MarkWin2DControl control && control.DataContext is Release release)
			{
				control.Text = release.Body ?? string.Empty;
			}
		}
	}
}
