using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StarSorter.Models;
using StarSorter.ViewModels;

namespace StarSorter.Controls
{
    public sealed partial class CommitCard : UserControl
    {
        public NotificationThread? ViewModel => DataContext as NotificationThread;

        public CommitCard()
        {
            this.InitializeComponent();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is NotificationThread thread)
            {
                var vm = App.Current.Services.GetRequiredService<NotificationsViewModel>();
                await vm.DeleteNotificationAsync(thread);
            }
        }
    }
}