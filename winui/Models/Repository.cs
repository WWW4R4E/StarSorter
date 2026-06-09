using System.Collections.Generic;

namespace StarSorter.ViewModels
{
    public class Repository
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string HtmlUrl { get; set; } = string.Empty;
        public int StargazersCount { get; set; }
        public int ForksCount { get; set; }
        public string? Language { get; set; }
        public List<string> Topics { get; set; } = new();
        public GitHubUser Owner { get; set; } = new();
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string PushedAt { get; set; } = string.Empty;
        public License? License { get; set; }
        public string? Homepage { get; set; }
        public bool Private { get; set; }
        public bool Fork { get; set; }
        public int OpenIssuesCount { get; set; }
        public int WatchersCount { get; set; }
        public string? Visibility { get; set; }
        public string DefaultBranch { get; set; } = string.Empty;
    }

    public class License
    {
        public string? Key { get; set; }
        public string? Name { get; set; }
        public string? SpdxId { get; set; }
        public string? Url { get; set; }
        public string? NodeId { get; set; }
    }
}
