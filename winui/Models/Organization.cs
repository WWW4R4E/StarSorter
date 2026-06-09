namespace StarSorter.ViewModels
{
    public class Organization
    {
        public string Login { get; set; } = string.Empty;
        public long Id { get; set; }
        public string? AvatarUrl { get; set; }
        public string? HtmlUrl { get; set; }
        public string? Name { get; set; }
        public string? Company { get; set; }
        public string? Blog { get; set; }
        public string? Location { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public int? PublicRepos { get; set; }
        public int? PublicGists { get; set; }
        public int? Followers { get; set; }
        public int? Following { get; set; }
    }
}
