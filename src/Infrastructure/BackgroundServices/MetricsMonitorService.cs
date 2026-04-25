using Application.Common.Configurations;
using Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core_Infrastructure.BackgroundServices
{
    public class MetricsMonitorService : BackgroundService
    {
        private readonly IApiMetricsService _metrics;
        private readonly ILogger<MetricsMonitorService> _logger;
        private readonly IOptions<BackgroundServiceOptions> _options;

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
                _logger.LogInformation("Metrics monitor is disabled.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var provider in _metrics.GetStats())
                {
                    var stats = provider.Value;
                    var lifetimeAverageMs = stats.LifetimeAverage.TotalMilliseconds;
                    var recentAverageMs = stats.LastFiveMinutesAverage.TotalMilliseconds;

                    if (stats.RecentSampleCount >= 5 &&
                        lifetimeAverageMs > 0 &&
                        recentAverageMs > lifetimeAverageMs * 1.5)
                    {
                        _logger.LogWarning(
                            "Performance anomaly detected for {Provider}. Recent avg {RecentAvg}ms vs lifetime avg {LifetimeAvg}ms.",
                            provider.Key,
                            Math.Round(recentAverageMs, 2),
                            Math.Round(lifetimeAverageMs, 2));
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(Math.Max(15, _options.Value.Timer)), stoppingToken);
            }
        }
    }
}
