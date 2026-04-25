namespace Core_Infrastructure.BackgroundServices
{
    using Application.Common.Configurations;
    using Application.Interfaces;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class MetricsMonitorService : BackgroundService
    {
        private readonly IApiMetricsService _metrics;
        private readonly ILogger<MetricsMonitorService> _logger;
        private readonly IOptions<BackgroundServiceOptions> _options;
        private readonly string[] _providers = { "Weather", "News", "Twitter", "Spotify", "GitHub" };

        public MetricsMonitorService(
            IApiMetricsService metrics,
            ILogger<MetricsMonitorService> logger,
            IOptions<BackgroundServiceOptions> options)
        {
            _metrics = metrics;
            _logger = logger;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Value.IsActive)
            {
                _logger.LogInformation("Background service is disabled.");
                return;
            }

            _logger.LogInformation("Background service started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var provider in _providers)
                {
                    var stats = _metrics.GetProviderStats(provider);

                    // Check Epameinondas' conditions: > 20 samples AND 5-min avg > 1.5x lifetime avg
                    if (stats.RecentSampleCount >= 20)
                    {
                        var recentAvgMs = stats.LastFiveMinutesAverage.TotalMilliseconds;
                        var lifetimeAvgMs = stats.LifetimeAverage.TotalMilliseconds;

                        if (recentAvgMs > (1.5 * lifetimeAvgMs))
                        {
                            _logger.LogWarning(
                                "ANOMALY DETECTED: {Provider} is degraded. " +
                                "Recent Avg: {RecentAvg}ms | Lifetime Avg: {LifetimeAvg}ms | Samples: {Count}",
                                provider, Math.Round(recentAvgMs, 2), Math.Round(lifetimeAvgMs, 2), stats.RecentSampleCount);
                        }
                    }
                }
            }
        }
    }
}
