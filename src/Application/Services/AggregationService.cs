namespace Application.Services
{

    using Application.DTOs;
    using Application.Interfaces;
    using Domain.Entities;
    using System.Diagnostics;

    public class AggregationService : IAggregationService
    {
        private readonly IEnumerable<IExternalApiClient> _clients;
        private readonly IApiMetricsService _metricsService; // We will inject this in Step 3

        public AggregationService(IEnumerable<IExternalApiClient> clients,
            IApiMetricsService metricsService)
        {
            _clients = clients;
            _metricsService = metricsService;
        }

        public async Task<AggregatedResponse> AggregateDataAsync(AggregationRequest request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = new AggregatedResponse();

            // 1. Create a task for each provider
            var fetchTasks = _clients.Select(client => FetchAndMapAsync(client, request.Query, cancellationToken)).ToList();

            // 2. Parallel Fan-out: Wait for all to complete (Polly handles individual timeouts/retries)
            var results = await Task.WhenAll(fetchTasks);

            // 3. Process the results into the envelope
            foreach (var (providerName, isSuccess, items, latency) in results)
            {
                if (isSuccess && items != null)
                {
                    response.Items.AddRange(items);
                    response.ProviderStatuses.Add(providerName, "Success");
                }
                else
                {
                    response.ProviderStatuses.Add(providerName, "Degraded/Failed");
                }

                // NEW: Fire and forget the metric recording
                _metricsService.RecordMetric(providerName, latency, isSuccess);

            }

            stopwatch.Stop();
            response.TotalProcessingTime = stopwatch.Elapsed;

            // Apply sorting/filtering based on the AggregationRequest here before returning

            return response;
        }

        private async Task<(string Provider, bool IsSuccess, IEnumerable<UnifiedItem>? Items, TimeSpan Latency)>
    FetchAndMapAsync(IExternalApiClient client, string query, CancellationToken cancellationToken)
        {
            // Note: In a real scenario, you'd have specific mapping logic per provider.
            // For simplicity, we are assuming FetchDataAsync returns a dynamic or mapped object.
            var result = await client.FetchDataAsync<IEnumerable<UnifiedItem>>(query, cancellationToken);

            return (client.ProviderName, result.IsSuccess, result.Data, result.Latency);
        }
    }
}
