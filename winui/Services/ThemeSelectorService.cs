using Microsoft.UI.Xaml;
using StarSorter.Helpers;

namespace StarSorter.Services
{
    public class ThemeSelectorService
    {
        public ElementTheme CurrentTheme => ThemeHelper.RootTheme;

        public void Initialize(Window window)
        {
            // ThemeHelper.Initialize() already restored the saved theme
        }

        public void SetTheme(ElementTheme theme)
        {
            ThemeHelper.RootTheme = theme;
        }
    }
}
