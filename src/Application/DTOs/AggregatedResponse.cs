using Domain.Entities;

namespace Application.DTOs
{
    public class AggregatedResponse
    {
        public List<UnifiedItem> Items { get; set; } = new List<UnifiedItem>();
    }
}
