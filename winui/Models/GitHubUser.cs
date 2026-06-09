using System;
using System.Text.Json.Serialization;

namespace StarSorter.ViewModels
{
    public class GitHubUser
    {
        public string Login { get; set; } = string.Empty;
        public long Id { get; set; }
        public string? NodeId { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public string? GravatarId { get; set; }
        public string HtmlUrl { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Company { get; set; }
        public string? Blog { get; set; }
        public string? Location { get; set; }
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public string? TwitterUsername { get; set; }
        public int? PublicRepos { get; set; }
        public int? PublicGists { get; set; }
        public int? Followers { get; set; }
        public int? Following { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public UserPlan? Plan { get; set; }

        [JsonIgnore]
        public Uri? BlogUri => !string.IsNullOrEmpty(Blog) ? new Uri(Blog) : null;

        [JsonIgnore]
        public Uri? EmailUri => !string.IsNullOrEmpty(Email) ? new Uri($"mailto:{Email}") : null;
    }

    public class UserPlan
    {
        public string Name { get; set; } = string.Empty;
        public long Space { get; set; }
        public int Collaborators { get; set; }
        public int PrivateRepos { get; set; }
    }
}
