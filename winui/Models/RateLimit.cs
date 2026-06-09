namespace StarSorter.ViewModels
{
    public class RateLimitResponse
    {
        public RateLimitResources Resources { get; set; } = new();
    }

    public class RateLimitResources
    {
        public RateLimitItem Core { get; set; } = new();
        public RateLimitItem Search { get; set; } = new();
        public RateLimitItem GraphQl { get; set; } = new();
    }

    public class RateLimitItem
    {
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public long Reset { get; set; }
        public int Used { get; set; }
    }
}
