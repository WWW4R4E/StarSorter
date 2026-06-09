using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StarSorter.Helpers;
using StarSorter.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace StarSorter.Views
{
    public sealed partial class AIClassifyContent : UserControl
    {
        private readonly StarsViewModel _viewModel;
        private ClassifyImportData? _importedData;

        public ObservableCollection<PreviewCategory> PreviewCategories { get; } = new();
        public ObservableCollection<string> CurrentCategoryRepos { get; } = new();

        public bool HasData => _importedData != null;

        public AIClassifyContent(StarsViewModel viewModel)
        {
            this.InitializeComponent();
            _viewModel = viewModel;
            ExportButton.Content = LocalizationHelper.GetLocalizedString("ExportData");
            ImportButton.Content = LocalizationHelper.GetLocalizedString("ImportResult");
            StatusText.Text = LocalizationHelper.GetLocalizedString("NoDataImported");
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var uncategorized = _viewModel.AllStars
                .Where(r => string.IsNullOrEmpty(r.Category))
                .ToList();

            if (uncategorized.Count == 0)
            {
                StatusText.Text = LocalizationHelper.GetLocalizedString("NoUncategorizedRepos");
                return;
            }

            var json = AIServiceHelper.BuildExportJson(uncategorized, _viewModel.NavLinks.ToList());

            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("JSON 文件", new List<string> { ".json" });
            savePicker.SuggestedFileName = "stars_classify_export.json";

            var window = WindowHelper.GetWindowForElement(this);
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                await FileIO.WriteTextAsync(file, json);
                StatusText.Text = LocalizationHelper.GetLocalizedString("ExportedCount", uncategorized.Count, file.Name);
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".json");

            var window = WindowHelper.GetWindowForElement(this);
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            var file = await openPicker.PickSingleFileAsync();
            if (file == null) return;

            var json = await FileIO.ReadTextAsync(file);
            _importedData = AIServiceHelper.ParseImportJson(json);

            if (!AIServiceHelper.ValidateImportData(_importedData, out var error))
            {
                StatusText.Text = error ?? LocalizationHelper.GetLocalizedString("ImportFailed");
                PreviewPanel.Visibility = Visibility.Collapsed;
                return;
            }

            PopulatePreview();
        }

        private void PopulatePreview()
        {
            if (_importedData == null) return;

            PreviewCategories.Clear();

            var uncategorizedRepos = _viewModel.AllStars
                .Where(r => string.IsNullOrEmpty(r.Category))
                .ToList();

            foreach (var kv in _importedData.Categories)
            {
                var cat = new PreviewCategory { Label = kv.Key };
                foreach (var repoName in kv.Value)
                {
                    cat.Repos.Add(uncategorizedRepos.FirstOrDefault(r =>
                        r.Name.Equals(repoName.Trim(), StringComparison.OrdinalIgnoreCase))?.Name ?? repoName);
                }
                PreviewCategories.Add(cat);
            }

            if (_importedData.Unassigned.Count > 0)
            {
                var unassigned = new PreviewCategory { Label = LocalizationHelper.GetLocalizedString("Unassign") };
                foreach (var name in _importedData.Unassigned)
                    unassigned.Repos.Add(name);
                PreviewCategories.Add(unassigned);
            }

            if (PreviewCategories.Count > 0)
                CategoryListView.SelectedIndex = 0;

            var totalRepoCount = _importedData.Categories.Sum(kv => kv.Value.Count);
            StatusText.Text = LocalizationHelper.GetLocalizedString("ImportedCount", totalRepoCount, _importedData.Categories.Count);
            SummaryText.Text = _importedData.Unassigned.Count > 0
                ? LocalizationHelper.GetLocalizedString("SummaryWithUnassigned", _importedData.Categories.Count, totalRepoCount, _importedData.Unassigned.Count)
                : LocalizationHelper.GetLocalizedString("SummaryText", _importedData.Categories.Count, totalRepoCount);

            PreviewPanel.Visibility = Visibility.Visible;
        }

        private void CategoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentCategoryRepos.Clear();

            if (CategoryListView.SelectedItem is PreviewCategory cat)
            {
                foreach (var repo in cat.Repos)
                    CurrentCategoryRepos.Add(repo);
            }
        }

        public async System.Threading.Tasks.Task<bool> TryApplyAsync()
        {
            if (_importedData == null)
                return false;

            ExportButton.IsEnabled = false;
            ImportButton.IsEnabled = false;

            try
            {
                foreach (var kv in _importedData.Categories)
                {
                    var categoryName = kv.Key;
                    var isNew = _viewModel.NavLinks.All(n =>
                        !n.Label.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

                    if (isNew)
                    {
                        _viewModel.NewCategoryName = categoryName;
                        await _viewModel.AddNewCategory();
                    }

                    foreach (var repoName in kv.Value)
                    {
                        var star = _viewModel.AllStars.FirstOrDefault(r =>
                            r.Name.Equals(repoName.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (star != null)
                            _viewModel.MoveStarToCategory(star, categoryName);
                    }
                }

                await _viewModel.SaveCategoriesAsync(
                    _viewModel.AllStars.ToList(),
                    _viewModel.NavLinks.ToList());

                if (_viewModel.SelectedCategory != null)
                    _viewModel.LoadStarsForCategory(_viewModel.SelectedCategory.Label);

                return true;
            }
            finally
            {
                ExportButton.IsEnabled = true;
                ImportButton.IsEnabled = true;
            }
        }
    }
}
