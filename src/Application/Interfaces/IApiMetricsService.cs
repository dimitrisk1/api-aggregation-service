using Application.DTOs;

namespace Application.Interfaces
{
    public interface IApiMetricsService
    {
        void RecordMetric(string providerName, TimeSpan latency, bool isSuccess);
        ApiStatsDto GetApiStatistics();
        ProviderStats GetProviderStats(string providerName);
        IReadOnlyDictionary<string, ProviderStats> GetStats();
    }
}
