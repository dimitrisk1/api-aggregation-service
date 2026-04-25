namespace Application.DTOs
{
    public class AggregationRequest
    {
        public string Query { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Source { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public string SortBy { get; set; } = "date";
        public string SortDirection { get; set; } = "desc";
        public int Limit { get; set; } = 25;
    }
}
