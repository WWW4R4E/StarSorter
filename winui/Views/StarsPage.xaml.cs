using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using StarSorter.Helpers;
using StarSorter.Services;
using StarSorter.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StarSorter.Views
{
	public sealed partial class StarsPage : Page
	{
		public StarsViewModel ViewModel { get; }
		private NavigationService NavigationService { get; }
		private bool _isMultiSelectMode = false;

		public StarsPage()
		{
			this.InitializeComponent();
			ViewModel = App.Current.Services.GetRequiredService<StarsViewModel>();
			NavigationService = App.Current.Services.GetRequiredService<NavigationService>();
			this.Loaded += StarsPage_Loaded;
		}

		public static Visibility ToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

		public static Visibility ToInvertedVisibility(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

		private void RetryButton_Click(object sender, RoutedEventArgs e)
		{
			ViewModel.HasError = false;
			ViewModel.ErrorMessage = null;
			_ = ViewModel.ReloadAsync();
		}

		private void StarsPage_Loaded(object sender, RoutedEventArgs e)
		{
			AddCategoryButton.Content = LocalizationHelper.GetLocalizedString("AddCategory");
			SmartClassifyButton.Content = LocalizationHelper.GetLocalizedString("SmartClassify");
			MultiSelectButton.Content = LocalizationHelper.GetLocalizedString("MultiSelect");

			if (CategoryNavLinksList.Items.Count > 0)
			{
				CategoryNavLinksList.SelectedIndex = 0;
				if (CategoryNavLinksList.Items[0] is NavLink firstLink)
					ViewModel.SelectedCategory = firstLink;
			}
		}

		private void CategoryNavLinksList_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (CategoryNavLinksList.SelectionMode != ListViewSelectionMode.Single)
				return;

			if (e.ClickedItem is NavLink selectedLink)
			{
				ViewModel.SelectedCategory = selectedLink;
				ViewModel.LoadStarsForCategory(selectedLink.Label);
			}
		}

		private void StarsGridView_ItemClick(object sender, ItemClickEventArgs e)
		{
			if (e.ClickedItem is StarRepository repo)
			{
				if (StarsGridView.SelectionMode == ListViewSelectionMode.None)
				{
					NavigationService.NavigateTo("StarDetail", repo);
				}
			}
		}

		private void StarsGridView_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
		{
			var element = e.OriginalSource as FrameworkElement;
			if (element?.DataContext is StarRepository repo)
			{
				ShowContextMenu(e.GetPosition(this), repo);
			}
		}

		private void Page_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
		{
			var element = e.OriginalSource as FrameworkElement;
			if (element != null && element.DataContext is StarRepository repo)
			{
				return;
			}

			if (_isMultiSelectMode && StarsGridView.SelectedItems.Count > 0)
			{
				ShowContextMenu(e.GetPosition(this), null);
			}
		}

		private void ShowContextMenu(Windows.Foundation.Point position, StarRepository? repo)
		{
			var menuFlyout = new MenuFlyout();

			Func<List<StarRepository>> targetItems = () => GetTargetItems(repo);

			var unstarItem = new MenuFlyoutItem
			{
				Text = LocalizationHelper.GetLocalizedString("Unstar"),
				Icon = new SymbolIcon(Symbol.UnFavorite)
			};
			unstarItem.Click += async (s, e) =>
			{
				var items = targetItems();
				if (items.Count == 0) return;
				var success = await ViewModel.UnstarRepositoriesAsync(items);
				if (!success)
					await ShowUnstarFailedDialog();
				CancelMultiSelect();
			};
			menuFlyout.Items.Add(unstarItem);

			menuFlyout.Items.Add(new MenuFlyoutSeparator());

			var currentCategory = ViewModel.SelectedCategory;
			if (currentCategory != null && !currentCategory.IsBuiltIn)
			{
				var removeItem = new MenuFlyoutItem
				{
					Text = LocalizationHelper.GetLocalizedString("RemoveFromCategory"),
					Icon = new SymbolIcon(Symbol.Remove)
				};
				removeItem.Click += async (s, e) =>
				{
					var items = targetItems();
					foreach (var item in items)
						ViewModel.RemoveStarFromCategory(item);
					await ViewModel.SaveCategoriesAsync(ViewModel.AllStars.ToList(), ViewModel.NavLinks.ToList());
					ViewModel.LoadStarsForCategory(currentCategory.Label);
					if (_isMultiSelectMode)
						CancelMultiSelect();
				};
				menuFlyout.Items.Add(removeItem);
				menuFlyout.Items.Add(new MenuFlyoutSeparator());
			}

			var addToCategorySubItem = new MenuFlyoutSubItem { Text = LocalizationHelper.GetLocalizedString("AddToCategory") };
			var categories = ViewModel.NavLinks.Where(n => !n.IsBuiltIn).ToList();
			foreach (var category in categories)
			{
				var menuItem = new MenuFlyoutItem
				{
					Text = category.Label,
					Tag = category.Label
				};
				var capturedCategory = category.Label;
				menuItem.Click += async (s, e) =>
				{
					var items = targetItems();
					foreach (var item in items)
						ViewModel.MoveStarToCategory(item, capturedCategory);
					await ViewModel.SaveCategoriesAsync(ViewModel.AllStars.ToList(), ViewModel.NavLinks.ToList());
					if (ViewModel.SelectedCategory?.Label is { } label)
						ViewModel.LoadStarsForCategory(label);
					if (_isMultiSelectMode)
						CancelMultiSelect();
				};
				addToCategorySubItem.Items.Add(menuItem);
			}
			menuFlyout.Items.Add(addToCategorySubItem);

			menuFlyout.ShowAt(this, position);
		}

		private List<StarRepository> GetTargetItems(StarRepository? repo)
		{
			if (_isMultiSelectMode && StarsGridView.SelectedItems.Count > 0)
				return StarsGridView.SelectedItems.OfType<StarRepository>().ToList();
			if (repo != null)
				return new List<StarRepository> { repo };
			return new List<StarRepository>();
		}

		private void CancelMultiSelect()
		{
			StarsGridView.SelectionMode = ListViewSelectionMode.None;
			_isMultiSelectMode = false;
			MultiSelectButton.Content = LocalizationHelper.GetLocalizedString("MultiSelect");
		}

		private void EnterMultiSelect()
		{
			StarsGridView.SelectionMode = ListViewSelectionMode.Multiple;
			_isMultiSelectMode = true;
			MultiSelectButton.Content = LocalizationHelper.GetLocalizedString("Cancel");
		}

		private async Task ShowUnstarFailedDialog()
		{
			var dialog = UIHelper.CreateThemedDialog(this.XamlRoot);
			dialog.Title = LocalizationHelper.GetLocalizedString("UnstarFailed");
			dialog.Content = LocalizationHelper.GetLocalizedString("UnstarFailedContent");
			dialog.PrimaryButtonText = LocalizationHelper.GetLocalizedString("OK");

			await dialog.ShowAsync();
		}

		private void CategoryNavLinksList_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
		{
			var element = e.OriginalSource as FrameworkElement;
			var navLink = element?.DataContext as NavLink;

			if (navLink == null)
				return;

			var isCategoryMulti = CategoryNavLinksList.SelectionMode != ListViewSelectionMode.Single;
			var menuFlyout = new MenuFlyout();

			var targetLabels = isCategoryMulti
				? CategoryNavLinksList.SelectedItems.OfType<NavLink>().Select(n => n.Label).ToList()
				: new List<string> { navLink.Label };

			var multiItem = new MenuFlyoutItem
			{
				Text = isCategoryMulti
					? LocalizationHelper.GetLocalizedString("CancelMultiSelect")
					: LocalizationHelper.GetLocalizedString("MultiSelect"),
				Icon = new SymbolIcon(isCategoryMulti ? Symbol.Cancel : Symbol.Bullets)
			};
			multiItem.Click += (s, args) =>
			{
				if (isCategoryMulti)
				{
					CategoryNavLinksList.SelectedItems.Clear();
					CategoryNavLinksList.SelectionMode = ListViewSelectionMode.Single;
				}
				else
				{
					CategoryNavLinksList.SelectionMode = ListViewSelectionMode.Multiple;
				}
			};
			menuFlyout.Items.Add(multiItem);

			if (!navLink.IsBuiltIn)
			{
				menuFlyout.Items.Add(new MenuFlyoutSeparator());

				var deleteItem = new MenuFlyoutItem
				{
					Text = LocalizationHelper.GetLocalizedString("DeleteCategory"),
					Icon = new SymbolIcon(Symbol.Delete)
				};
				deleteItem.Click += async (s, args) =>
				{
					foreach (var label in targetLabels)
						await ViewModel.DeleteCategory(label);
				};
				menuFlyout.Items.Add(deleteItem);
			}

			menuFlyout.Items.Add(new MenuFlyoutSeparator());

			var resetAllItem = new MenuFlyoutItem
			{
				Text = LocalizationHelper.GetLocalizedString("ResetCategory"),
				Icon = new SymbolIcon(Symbol.Undo)
			};
			resetAllItem.Click += async (s, args) =>
			{
				var allStarsLabel = ViewModel.NavLinks
					.FirstOrDefault(n => n.IsBuiltIn && n.Glyph == "\uE734")?.Label ?? "All Stars";
				var userCategories = ViewModel.NavLinks.Where(n => !n.IsBuiltIn).ToList();
				foreach (var cat in userCategories)
				{
					foreach (var star in ViewModel.AllStars.Where(s => s.Category == cat.Label))
						ViewModel.RemoveStarFromCategory(star);
					ViewModel.NavLinks.Remove(cat);
				}
				ViewModel.UpdateUncategorizedLink();
				await ViewModel.SaveCategoriesAsync(ViewModel.AllStars.ToList(), ViewModel.NavLinks.ToList());
				ViewModel.LoadStarsForCategory(ViewModel.SelectedCategory?.Label ?? allStarsLabel);
			};
			menuFlyout.Items.Add(resetAllItem);

			menuFlyout.ShowAt(sender as ListView, e.GetPosition(sender as ListView));
		}

		private async void AddCategoryButton_Click(object sender, RoutedEventArgs e)
		{
			await ShowAddCategoryDialog();
		}

		private async void SmartClassifyButton_Click(object sender, RoutedEventArgs e)
		{
			var content = new AIClassifyContent(ViewModel);
			var dialog = UIHelper.CreateThemedDialog(this.XamlRoot);
			dialog.Title = LocalizationHelper.GetLocalizedString("SmartClassify");
			dialog.DefaultButton = ContentDialogButton.None;
			dialog.PrimaryButtonText = LocalizationHelper.GetLocalizedString("ConfirmExecute");
			dialog.PrimaryButtonStyle = (Style)Application.Current.Resources["AccentButtonStyle"];
			dialog.CloseButtonText = LocalizationHelper.GetLocalizedString("Cancel");
			dialog.Content = content;

			dialog.PrimaryButtonClick += async (s, args) =>
			{
				if (!content.HasData)
				{
					args.Cancel = true;
					return;
				}

				var deferral = args.GetDeferral();
				var success = await content.TryApplyAsync();
				deferral.Complete();
			};

			await dialog.ShowAsync();
		}

		private void MultiSelectButton_Click(object sender, RoutedEventArgs e)
		{
			if (_isMultiSelectMode)
				CancelMultiSelect();
			else
				EnterMultiSelect();
		}

		private async Task ShowAddCategoryDialog()
		{
			var dialog = UIHelper.CreateThemedDialog(this.XamlRoot);
			dialog.Title = LocalizationHelper.GetLocalizedString("AddCategoryDialogTitle");
			dialog.PrimaryButtonText = LocalizationHelper.GetLocalizedString("OK");
			dialog.CloseButtonText = LocalizationHelper.GetLocalizedString("Cancel");

			var textBox = new TextBox
			{
				PlaceholderText = LocalizationHelper.GetLocalizedString("CategoryNamePlaceholder")
			};
			dialog.Content = textBox;
			textBox.Loaded += (s, e) => textBox.Focus(FocusState.Programmatic);

			if (await dialog.ShowAsync() == ContentDialogResult.Primary)
			{
				ViewModel.NewCategoryName = textBox.Text;
				await ViewModel.AddNewCategory();
			}
		}

		private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
		{
			if (parent == null) return null;

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child != null && child is T result)
				{
					return result;
				}

				var childOfChild = FindVisualChild<T>(child);
				if (childOfChild != null)
					return childOfChild;
			}

			return null;
		}

		public void SearchStars(string query)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				// 如果查询为空，显示默认分类的内容
				if (CategoryNavLinksList.SelectedItem is NavLink selectedLink)
				{
					ViewModel.LoadStarsForCategory(selectedLink.Label);
				}
				else
				{
					ViewModel.LoadStarsForCategory(LocalizationHelper.GetLocalizedString("AllStars"));
				}
				return;
			}

			// 过滤当前显示的星标仓库
			var filteredStars = ViewModel.CurrentCategoryStars
				.Where(repo =>
					repo.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
					repo.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) == true ||
					repo.Topics.Any(topic => topic.Contains(query, StringComparison.OrdinalIgnoreCase)))
				.ToList();

			// 创建一个新的集合来替换当前显示的仓库
			var filteredCollection = new ObservableCollection<StarRepository>(filteredStars);
			ViewModel.CurrentCategoryStars.Clear();
			foreach (var repo in filteredCollection)
			{
				ViewModel.CurrentCategoryStars.Add(repo);
			}
		}
	}
}