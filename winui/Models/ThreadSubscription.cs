using System.Text.Json.Serialization;

namespace StarSorter.Models
{
    public class ThreadSubscription
    {
        [JsonPropertyName("subscribed")]
        public bool Subscribed { get; set; }

        [JsonPropertyName("ignored")]
        public bool Ignored { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("thread_url")]
        public string? ThreadUrl { get; set; }

        [JsonPropertyName("repository_url")]
        public string? RepositoryUrl { get; set; }
    }
}