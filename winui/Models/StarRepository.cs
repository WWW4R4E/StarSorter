using System.Collections.Generic;

namespace StarSorter.ViewModels
{
    public class StarRepository
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string HtmlUrl { get; set; } = string.Empty;
        public string Stars { get; set; } = string.Empty;
        public string Forks { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? Language { get; set; }
        public List<string> Topics { get; set; } = new();
        public string? OwnerName { get; set; }
        public string? Category { get; set; }

        public static StarRepository FromRepository(Repository repo)
        {
            return new StarRepository
            {
                Id = repo.Id,
                Name = repo.Name,
                FullName = repo.FullName,
                Description = repo.Description,
                HtmlUrl = repo.HtmlUrl,
                Stars = FormatCount(repo.StargazersCount),
                Forks = FormatCount(repo.ForksCount),
                ImageUrl = repo.Owner?.AvatarUrl ?? string.Empty,
                Language = repo.Language,
                Topics = repo.Topics ?? new(),
                OwnerName = repo.Owner?.Login,
            };
        }

        private static string FormatCount(int count)
        {
            if (count >= 1000)
                return $"{count / 1000.0:F1}k";
            return count.ToString();
        }
    }
}
