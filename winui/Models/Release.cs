using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace StarSorter.ViewModels
{
    public class Release : INotifyPropertyChanged
    {
        public long Id { get; set; }
        public string TagName { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Body { get; set; }
        public string? PublishedAt { get; set; }
        public string HtmlUrl { get; set; } = string.Empty;
        public bool Prerelease { get; set; }

        [JsonIgnore]
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _isExpanded;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
