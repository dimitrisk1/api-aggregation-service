using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System.Diagnostics;

namespace Application.Services
{
    public class AggregationService : IAggregationService
    {
        private readonly IEnumerable<IExternalProvider> _providers;
        private readonly IApiMetricsService _metricsService;

        public AggregationService(IEnumerable<IExternalProvider> providers, IApiMetricsService metricsService)
        {
            _providers = providers;
            _metricsService = metricsService;
        }

        public async Task<AggregatedResponse> AggregateDataAsync(AggregationRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                throw new ArgumentException("Query is required.", nameof(request.Query));
            }

            var stopwatch = Stopwatch.StartNew();
            var results = await Task.WhenAll(
                _providers.Select(provider => FetchAndMapAsync(provider, request.Query, cancellationToken)));

            var response = new AggregatedResponse
            {
                Query = request.Query,
                GeneratedAtUtc = DateTime.UtcNow
            };

            var allItems = new List<UnifiedItem>();

            foreach (var result in results)
            {
                if (result.Items is not null)
                {
                    allItems.AddRange(result.Items);
                }

                response.Providers.Add(new ProviderExecutionDto
                {
                    ProviderName = result.Provider,
                    Status = result.IsSuccess ? (result.IsFallback ? "Fallback" : "Success") : "Failed",
                    ItemCount = result.Items?.Count() ?? 0,
                    ResponseTimeMs = Math.Round(result.Latency.TotalMilliseconds, 2),
                    ErrorMessage = result.ErrorMessage
                });

                _metricsService.RecordMetric(result.Provider, result.Latency, result.IsSuccess && !result.IsFallback);
            }

            stopwatch.Stop();
            response.Items = ApplyFiltersAndSorting(allItems, request).ToList();
            response.TotalItems = response.Items.Count;
            response.TotalProcessingTimeMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);

            return response;
        }

        private static IEnumerable<UnifiedItem> ApplyFiltersAndSorting(IEnumerable<UnifiedItem> items, AggregationRequest request)
        {
            var filtered = items;

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                filtered = filtered.Where(item => string.Equals(item.Category, request.Category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.Source))
            {
                filtered = filtered.Where(item => string.Equals(item.Source, request.Source, StringComparison.OrdinalIgnoreCase));
            }

            if (request.FromUtc.HasValue)
            {
                filtered = filtered.Where(item => item.Date >= request.FromUtc.Value);
            }

            if (request.ToUtc.HasValue)
            {
                filtered = filtered.Where(item => item.Date <= request.ToUtc.Value);
            }

            var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            filtered = request.SortBy.ToLowerInvariant() switch
            {
                "relevance" => descending ? filtered.OrderByDescending(item => item.RelevanceScore) : filtered.OrderBy(item => item.RelevanceScore),
                "title" => descending ? filtered.OrderByDescending(item => item.Title) : filtered.OrderBy(item => item.Title),
                "source" => descending ? filtered.OrderByDescending(item => item.Source) : filtered.OrderBy(item => item.Source),
                _ => descending ? filtered.OrderByDescending(item => item.Date) : filtered.OrderBy(item => item.Date)
            };

            var limit = request.Limit <= 0 ? 25 : Math.Min(request.Limit, 100);
            return filtered.Take(limit);
        }

        private static async Task<(string Provider, bool IsSuccess, bool IsFallback, IEnumerable<UnifiedItem>? Items, TimeSpan Latency, string? ErrorMessage)>
            FetchAndMapAsync(IExternalProvider provider, string query, CancellationToken cancellationToken)
        {
            var result = await provider.FetchDataAsync(query, cancellationToken);
            return (provider.ProviderName, result.IsSuccess, result.IsFallback, result.Data, result.Latency, result.ErrorMessage);
        }
    }
}
