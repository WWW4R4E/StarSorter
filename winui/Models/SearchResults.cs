using System.Collections.Generic;

namespace StarSorter.ViewModels
{
    public class SearchResults<T>
    {
        public long TotalCount { get; set; }
        public bool IncompleteResults { get; set; }
        public List<T> Items { get; set; } = new();
    }
}
