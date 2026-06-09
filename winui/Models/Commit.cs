namespace StarSorter.ViewModels
{
    public class Commit
    {
        public string Sha { get; set; } = string.Empty;
        public CommitCommit? CommitInfo { get; set; }
        public string HtmlUrl { get; set; } = string.Empty;
        public GitHubUser? Author { get; set; }
        public GitHubUser? Committer { get; set; }
    }

    public class CommitCommit
    {
        public CommitAuthor? Author { get; set; }
        public CommitAuthor? Committer { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CommitAuthor
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
    }
}
