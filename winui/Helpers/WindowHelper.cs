using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace StarSorter.Helpers
{
    public partial class WindowHelper
    {
        static public Window CreateWindow()
        {
            MainWindow newWindow = new MainWindow();
            TrackWindow(newWindow);
            return newWindow;
        }

        static public void TrackWindow(Window window)
        {
            window.Closed += (sender, args) =>
            {
                _activeWindows.Remove(window);

                // If no more windows are active, cleanup theme helpers
                if (_activeWindows.Count == 0)
                {
                    ThemeHelper.Cleanup();
                }
            };
            _activeWindows.Add(window);
        }

        static public Window GetWindowForElement(UIElement element)
        {
            if (element.XamlRoot != null)
            {
                foreach (Window window in _activeWindows)
                {
                    if (window.Content != null && element.XamlRoot == window.Content.XamlRoot)
                    {
                        return window;
                    }
                }
            }

            // 如果没有找到匹配的XamlRoot，在大多数单窗口应用中返回第一个活动窗口
            if (_activeWindows.Count > 0)
            {
                return _activeWindows[0];
            }

            return null!;
        }

        static public double GetRasterizationScaleForElement(UIElement element)
        {
            if (element.XamlRoot != null)
            {
                foreach (Window window in _activeWindows)
                {
                    if (element.XamlRoot == window.Content.XamlRoot)
                    {
                        return element.XamlRoot.RasterizationScale;
                    }
                }
            }
            return 0.0;
        }

        static public void SetWindowMinSize(Window window, double width, double height)
        {
            if (window.Content is not FrameworkElement windowContent)
            {
                System.Diagnostics.Debug.WriteLine("Window content is not a FrameworkElement.");
                return;
            }

            if (windowContent.XamlRoot is null)
            {
                System.Diagnostics.Debug.WriteLine("Window content's XamlRoot is null.");
                return;
            }

            if (window.AppWindow.Presenter is not OverlappedPresenter presenter)
            {
                System.Diagnostics.Debug.WriteLine("Window's AppWindow.Presenter is not an OverlappedPresenter.");
                return;
            }

            var scale = windowContent.XamlRoot.RasterizationScale;
            var minWidth = width * scale;
            var minHeight = height * scale;
            presenter.PreferredMinimumWidth = (int)minWidth;
            presenter.PreferredMinimumHeight = (int)minHeight;
        }

        static public List<Window> ActiveWindows { get { return _activeWindows; } }

        static private List<Window> _activeWindows = new List<Window>();
    }

}