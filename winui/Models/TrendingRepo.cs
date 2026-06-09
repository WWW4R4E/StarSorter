namespace StarSorter.ViewModels
{
    public class TrendingRepo
    {
        public int Rank { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Stars { get; set; } = string.Empty;
        public string Forks { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string SpokenLanguage { get; set; } = string.Empty;
        public string TodayStars { get; set; } = string.Empty;
    }
}
