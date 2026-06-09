namespace StarSorter.ViewModels
{
    public class Workflow
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Path { get; set; }
        public string State { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
    }
}
