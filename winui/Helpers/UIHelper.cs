using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Foundation.Metadata;

namespace StarSorter.Helpers
{
	public static partial class UIHelper
	{
		static UIHelper()
		{
		}

		public static ContentDialog CreateThemedDialog(XamlRoot xamlRoot)
		{
			var resolvedTheme = ThemeHelper.RootTheme == ElementTheme.Default
				? ThemeHelper.ActualTheme
				: ThemeHelper.RootTheme;

			return new ContentDialog
			{
				XamlRoot = xamlRoot,
				RequestedTheme = resolvedTheme
			};
		}

		public static Visibility IsNotEmpty(string value) =>
			string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;

		public static IEnumerable<T> GetDescendantsOfType<T>(this DependencyObject start) where T : DependencyObject
		{
			return start.GetDescendants().OfType<T>();
		}

		public static IEnumerable<DependencyObject> GetDescendants(this DependencyObject start)
		{
			var queue = new Queue<DependencyObject>();
			var count1 = VisualTreeHelper.GetChildrenCount(start);

			for (int i = 0; i < count1; i++)
			{
				var child = VisualTreeHelper.GetChild(start, i);
				yield return child;
				queue.Enqueue(child);
			}

			while (queue.Count > 0)
			{
				var parent = queue.Dequeue();
				var count2 = VisualTreeHelper.GetChildrenCount(parent);

				for (int i = 0; i < count2; i++)
				{
					var child = VisualTreeHelper.GetChild(parent, i);
					yield return child;
					queue.Enqueue(child);
				}
			}
		}

		static public UIElement? FindElementByName(UIElement element, string name)
		{
			if (element.XamlRoot != null && element.XamlRoot.Content != null)
			{
				var frameworkElement = element.XamlRoot.Content as FrameworkElement;
				if (frameworkElement != null)
				{
					var ele = frameworkElement.FindName(name);
					if (ele != null)
					{
						return ele as UIElement;
					}
				}
			}
			return null;
		}
	}
}