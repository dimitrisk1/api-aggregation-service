namespace Domain.Models
{
    public class ProviderResult<T>
    {
        public bool IsSuccess { get; set; }
        public bool IsFallback { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan Latency { get; set; }
    }
}
