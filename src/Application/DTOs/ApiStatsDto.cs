namespace Application.DTOs
{
    public class ApiStatsDto
    {
        public Dictionary<string, ApiStatItemDto> Providers { get; set; } = new();
    }

    public class ApiStatItemDto
    {
        public long TotalRequests { get; set; }
        public long SuccessfulRequests { get; set; }
        public long FailedRequests { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public double LastFiveMinutesAverageResponseTimeMs { get; set; }
        public Dictionary<string, long> Buckets { get; set; } = new();
    }
}
