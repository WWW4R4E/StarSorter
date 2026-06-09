using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using StarSorter.ViewModels;

namespace StarSorter.Views
{
    public sealed partial class ExplorePage : Page
    {
        public ExploreViewModel ViewModel { get; }

        public ExplorePage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<ExploreViewModel>();
        }
    }
}