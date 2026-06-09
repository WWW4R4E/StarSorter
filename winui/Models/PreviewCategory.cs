using System.Collections.ObjectModel;

namespace StarSorter.ViewModels
{
    public class PreviewCategory
    {
        public string Label { get; set; } = string.Empty;
        public ObservableCollection<string> Repos { get; set; } = new();

        public int RepoCount => Repos.Count;
    }
}
