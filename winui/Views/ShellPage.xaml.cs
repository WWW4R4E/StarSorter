using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StarSorter.Helpers;
using StarSorter.Services;
using StarSorter.ViewModels;
using System.Linq;


namespace StarSorter.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ShellPage : Page
	{
		public ShellViewModel ViewModel { get; }
		private readonly NavigationService _navigationService;

		public ShellPage()
		{
			this.InitializeComponent();
			ViewModel = App.Current.Services.GetRequiredService<ShellViewModel>();
			_navigationService = App.Current.Services.GetRequiredService<NavigationService>();

			ViewModel.DialogRequested += OnDialogRequested;

			_navigationService.Initialize(ContentFrame);
			NavView.Loaded += (_, _) =>
			{
				ViewModel.LoadPaneDisplayMode();
				NavView.SelectedItem = NavView.MenuItems.FirstOrDefault();
			};
			ContentFrame.Navigated += ViewModel.OnFrameNavigated;
		}

		private async void OnDialogRequested(object? sender, (string Title, string Message) e)
		{
			var dialog = UIHelper.CreateThemedDialog(this.XamlRoot);
			dialog.Title = e.Title;
			dialog.Content = e.Message;
			dialog.CloseButtonText = LocalizationHelper.GetLocalizedString("OK");
			await dialog.ShowAsync();
		}

		private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
		{
			_navigationService.GoBack();
		}

		private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (args.IsSettingsSelected)
			{
				_navigationService.NavigateTo("Settings", null, clearNavigation: true);
			}
			else if (args.SelectedItem is NavigationViewItem selectedItem && selectedItem.Tag is string tag)
			{
				if (!string.IsNullOrEmpty(tag))
				{
					_navigationService.NavigateTo(tag, null, clearNavigation: true);
				}
			}
		}

		private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (!string.IsNullOrEmpty(args.QueryText))
			{
				HandleSearchQuery(args.QueryText);
			}
		}

		private void HandleSearchQuery(string query)
		{
			if (ContentFrame.Content is StarsPage starsPage)
			{
				starsPage.SearchStars(query);
			}
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			var dataManager = App.Current.Services.GetRequiredService<DataManagerService>();

			if (ContentFrame.Content is StarsPage starsPage)
			{
				_ = Task.Run(async () =>
				{
					DispatcherQueue.TryEnqueue(() =>
					{
						starsPage.ViewModel?.LoadStarsForCategory(starsPage.ViewModel.SelectedCategory?.Label ?? "全部星星");
					});
				});
			}
			else if (ContentFrame.Content is NotificationsPage notificationsPage)
			{
				_ = Task.Run(async () =>
				{
					DispatcherQueue.TryEnqueue(() =>
					{
						notificationsPage.ViewModel?.RefreshNotificationsAsync();
          });
				});
			}
			// TODO: 暂时隐藏 - ExplorePage 尚未完善
			//else if (ContentFrame.Content is ExplorePage explorePage)
			//{
			//	_ = Task.Run(async () =>
			//	{
			//		DispatcherQueue.TryEnqueue(() =>
			//		{
			//			explorePage.ViewModel?.LoadExploreChannels();
			//		});
			//	});
			//}
			else if (ContentFrame.Content is ProfilePage profilePage)
			{
				_ = Task.Run(async () =>
				{
					DispatcherQueue.TryEnqueue(() =>
					{
						profilePage.ViewModel?.LoadUserProfile();
					});
				});
			}
		}
	}
}