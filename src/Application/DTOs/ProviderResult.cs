namespace Application.DTOs
{
    public class ProviderResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan Latency { get; set; }
    }
}
