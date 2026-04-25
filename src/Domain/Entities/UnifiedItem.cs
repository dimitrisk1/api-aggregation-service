namespace Domain.Entities
{
    public class UnifiedItem
    {
        public string Source { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }
        public DateTime Date { get; set; }
    }
}
