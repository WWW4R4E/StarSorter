using System.Text.Json.Serialization;

namespace StarSorter.Models
{
    public class MarkNotificationsResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}