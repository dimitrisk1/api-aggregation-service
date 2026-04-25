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
                var stats = _metrics.GetStats();

                foreach (var api in stats.APIs)
                {
                    var avg = api.Value.TotalResponseTime;

                    if (avg > 300)
                    {
                        _logger.LogWarning(
                            "Performance issue detected for {Api}: Avg = {Avg}ms",
                            api.Key, avg);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(_options.Value.Timer), stoppingToken);
            }
        }
    }
}
