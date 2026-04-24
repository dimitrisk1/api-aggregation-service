namespace Application.DTOs
{
    public class ApiStatsDto
    {
        public Dictionary<string, ApiStatItemDto> APIs { get; set; }
    }

    public class ApiStatItemDto
    {
        public long TotalRequests { get; set; }
        public double TotalResponseTime { get; set; }
        public Dictionary<string, int> Buckets { get; set; }
    }
}
