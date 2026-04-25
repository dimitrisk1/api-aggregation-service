using Application.DTOs;

namespace Application.Interfaces
{
    public interface IApiMetricsService
    {
        void RecordMetric(string providerName, TimeSpan latency, bool isSuccess);

        // We'll use this later for the Anomaly Detection background worker
        ProviderStats GetProviderStats(string providerName);
    }
}
