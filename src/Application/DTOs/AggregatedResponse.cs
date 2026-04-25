using Domain.Entities;

namespace Application.DTOs
{
    public class AggregatedResponse
    {
        public string Query { get; set; } = string.Empty;
        public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
        public int TotalItems { get; set; }
        public List<UnifiedItem> Items { get; set; } = new();
        public List<ProviderExecutionDto> Providers { get; set; } = new();
        public double TotalProcessingTimeMs { get; set; }
    }
}
