using System.Text.Json.Serialization;

namespace StarSorter.Models
{
    public class NotificationSubject
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("latest_comment_url")]
        public string LatestCommentUrl { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonIgnore]
        public bool IsIssue => Type == "Issue";

        [JsonIgnore]
        public bool IsPullRequest => Type == "PullRequest";

        [JsonIgnore]
        public bool IsRelease => Type == "Release";

        [JsonIgnore]
        public bool IsCommit => Type == "Commit";
    }
}