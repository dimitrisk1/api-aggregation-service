using Domain.Entities;

namespace Application.DTOs
{
    public class AggregatedResponse
    {
        public List<UnifiedItem> Items { get; set; } = new();

        public Dictionary<string, string> ProviderStatuses { get; set; } = new();

        public TimeSpan TotalProcessingTime { get; set; }
    }
}
