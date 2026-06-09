using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarSorter.Services
{
    public class NavigationService()
    {
        private readonly Dictionary<string, Type> _pages = new();
        private Frame? _frame;
        public event Action<string>? Navigated;
        // 逻辑后退栈
        private int _logicalBackStack = 0;
        public bool CanGoBack => _logicalBackStack > 0;

        private void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            if (_frame != null && !_frame.CanGoBack)
            {
                _logicalBackStack = 0;
            }
        }

        // 在App启动时配置页面路由
        public void Configure(string key, Type pageType)
        {
            lock (_pages)
            {
                if (_pages.ContainsKey(key))
                {
                    throw new ArgumentException($"The key {key} is already configured.");
                }
                _pages.Add(key, pageType);
            }
        }

        public void Initialize(Frame frame)
        {
            _frame = frame;
            _frame.Navigated += OnFrameNavigated;
        }
        public void GoBack()
        {
            if (CanGoBack)
            {
                _logicalBackStack--;
                _frame?.GoBack();
            }
        }

        public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
        {
            if (_frame == null)
            {
                throw new InvalidOperationException("Frame is not initialized.");
            }

            if (!_pages.TryGetValue(pageKey, out var pageType))
            {
                throw new ArgumentException($"Page not found: {pageKey}. Did you forget to call Configure?");
            }


            if (_frame.Content?.GetType() != pageType)
            {
                var result = _frame.Navigate(pageType, parameter);
                if (result)
                {
                    if (clearNavigation)
                    {
                        _frame.BackStack.Clear();
                        _logicalBackStack = 0;
                    }
                    else
                    {
                        _logicalBackStack++;
                    }
                    Navigated?.Invoke(pageKey);

                }
                return result;
            }

            return false;
        }
    }

}
