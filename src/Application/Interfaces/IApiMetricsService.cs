using Application.DTOs;

namespace Application.Interfaces
{
    public interface IApiMetricsService
    {
        void Record(string apiName, long responseTimeMs);
        ApiStatsDto GetStats();
    }
}
