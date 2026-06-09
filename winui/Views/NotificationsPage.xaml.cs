using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StarSorter.Models;
using StarSorter.ViewModels;
using System;
using Windows.System;

namespace StarSorter.Views
{
	public sealed partial class NotificationsPage : Page
	{
		public NotificationsViewModel ViewModel { get; }

		public NotificationsPage()
		{
			this.InitializeComponent();
			ViewModel = App.Current.Services.GetRequiredService<NotificationsViewModel>();
		}

		public static Visibility ToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

		public static Visibility ToInvertedVisibility(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

		private void NotificationsNavLinksList_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (e.ClickedItem is NavLink navLink)
			{
				ViewModel.LoadNotificationsForCategory(navLink.Label);
			}
		}

		private async void NotificationsListView_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (e.ClickedItem is NotificationThread thread && !string.IsNullOrEmpty(thread.Repository?.HtmlUrl))
			{
				await Launcher.LaunchUriAsync(new Uri(thread.Repository.HtmlUrl));
			}
		}

		private async void MarkAllReadButton_Click(object sender, RoutedEventArgs e)
		{
			await ViewModel.MarkAllReadAsync();
		}
	}
}
