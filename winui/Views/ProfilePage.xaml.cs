using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StarSorter.ViewModels;

namespace StarSorter.Views
{
    public sealed partial class ProfilePage : Page
    {
        public ProfileViewModel ViewModel { get; }

        public ProfilePage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<ProfileViewModel>();
        }

        public static Visibility ToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

        public static Visibility ToInvertedVisibility(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.HasError = false;
            ViewModel.ErrorMessage = null;
            ViewModel.LoadUserProfile();
        }
    }
}
