namespace Application.DTOs
{
    public class ProviderExecutionDto
    {
        public string ProviderName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public double ResponseTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
