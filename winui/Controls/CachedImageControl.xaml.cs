using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using StarSorter.Services;

namespace StarSorter.Controls
{
	public sealed partial class CachedImageControl : UserControl
	{
		public string Source
		{
			get => (string)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		public static readonly Microsoft.UI.Xaml.DependencyProperty SourceProperty =
			Microsoft.UI.Xaml.DependencyProperty.Register(
				nameof(Source),
				typeof(string),
				typeof(CachedImageControl),
				new Microsoft.UI.Xaml.PropertyMetadata(null, OnSourceChanged));

		public CachedImageControl()
		{
			this.InitializeComponent();
		}

		private static void OnSourceChanged(Microsoft.UI.Xaml.DependencyObject d, Microsoft.UI.Xaml.DependencyPropertyChangedEventArgs e)
		{
			if (d is CachedImageControl control)
			{
				control.LoadImageAsync((string)e.NewValue);
			}
		}

		private async void LoadImageAsync(string source)
		{
			if (string.IsNullOrEmpty(source) || ImageElement == null) return;

			try
			{
				var cacheService = App.Current.Services.GetService<ImageCacheService>();
				if (cacheService == null)
				{
					DispatcherQueue.TryEnqueue(() =>
						ImageElement.Source = new BitmapImage(new System.Uri(source)));
					return;
				}

				var cachedPath = await cacheService.GetCachedImagePathAsync(source);
				var bitmapImage = new BitmapImage(new System.Uri(cachedPath));

				DispatcherQueue.TryEnqueue(() =>
					ImageElement.Source = bitmapImage);
			}
			catch
			{
				DispatcherQueue.TryEnqueue(() =>
					ImageElement.Source = new BitmapImage(new System.Uri("ms-appx:///Assets/default-avatar.png")));
			}
		}
	}
}