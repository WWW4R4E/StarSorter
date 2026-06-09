using System.Text.Json;
using System.Text.Json.Serialization;
using StarSorter.Models;
using StarSorter.ViewModels;

namespace StarSorter.Helpers
{
    [JsonSerializable(typeof(CachedRepoDetail))]
    [JsonSerializable(typeof(List<Repository>))]
    [JsonSerializable(typeof(List<Contributor>))]
    [JsonSerializable(typeof(List<Release>))]
    [JsonSerializable(typeof(List<Commit>))]
    [JsonSerializable(typeof(List<Workflow>))]
    [JsonSerializable(typeof(List<NotificationThread>))]
    [JsonSerializable(typeof(List<StarRepository>))]
    [JsonSerializable(typeof(List<NavLink>))]
    [JsonSerializable(typeof(List<NotificationItem>))]
    [JsonSerializable(typeof(SearchResults<Repository>))]
    [JsonSerializable(typeof(Dictionary<string, long>))]
    [JsonSerializable(typeof(JsonElement))]
    [JsonSerializable(typeof(List<JsonElement>))]
    [JsonSerializable(typeof(Contributor))]
    [JsonSerializable(typeof(Release))]
    [JsonSerializable(typeof(GitHubUser))]
    [JsonSerializable(typeof(License))]
    [JsonSerializable(typeof(RateLimitResponse))]
    [JsonSerializable(typeof(Organization))]
    [JsonSerializable(typeof(ThreadSubscription))]
    [JsonSerializable(typeof(Commit))]
    [JsonSerializable(typeof(Workflow))]
    [JsonSerializable(typeof(NotificationThread))]
    [JsonSerializable(typeof(StarRepository))]
    [JsonSerializable(typeof(NavLink))]
    [JsonSerializable(typeof(NotificationItem))]
    [JsonSerializable(typeof(ClassifyExportData))]
    [JsonSerializable(typeof(ClassifyImportData))]
    public partial class SerializationContext : JsonSerializerContext
    {
        public static SerializationContext CamelCase { get; } = new SerializationContext(
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }
        );

        public static SerializationContext SnakeCaseLower { get; } = new SerializationContext(
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }
        );
    }

    public class CachedRepoDetail
    {
        public Repository Repository { get; set; } = new();
        public List<Contributor> Contributors { get; set; } = new();
        public List<Release> Releases { get; set; } = new();
        public string? Readme { get; set; }
        public DateTime CachedAt { get; set; }
    }
}