namespace Application.Services
{

    using Application.DTOs;
    using Application.Interfaces;
    using Domain.Entities;
    using Domain.Interfaces;
    using System.Diagnostics;
    using System.IO;


    public class AggregationService : IAggregationService
    {
        private readonly IEnumerable<IExternalProvider> _providers;
        private readonly IEnumerable<IExternalApiClient> _clients;
        private readonly IApiMetricsService _metrics;

        public AggregationService(
            IEnumerable<IExternalApiClient> clients,
            IApiMetricsService metrics)
            //            IEnumerable<IExternalProvider> providers)
        {
            _clients = clients;
            _metrics = metrics;
           // _providers = providers;
        }


        //public async Task<IEnumerable<AggregatedResponse>> GetUnifiedDataAsync(FilterOptions options)
        //{
        //    var tasks = _providers.Select(p => p.GetDataAsync(default));
        //    var results = await Task.WhenAll(tasks);

        //    // Logic for filtering/sorting stays here, NOT in the controller
        //    return results.SelectMany(x => x)
        //                  .Where(x => x.Category == options.Category)
        //                  .OrderBy(x => x.Date);
        //}
        public async Task<AggregatedResponse> HandleAsync(AggregationRequest request)
        {
            var tasks = _clients.Select(client => ExecuteClient(client, request));

            var results = await Task.WhenAll(tasks);

            var items = results.SelectMany(r => r);

            items = ApplyFiltering(items, request);

            items = ApplySorting(items, request);

            return new AggregatedResponse
            {
                Items = items.ToList()
            };
        }

        private async Task<IEnumerable<UnifiedItem>> ExecuteClient(
            IExternalApiClient client,
            AggregationRequest request)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = await client.FetchAsync(request);
                return result ?? Enumerable.Empty<UnifiedItem>();
            }
            catch
            {
                return Enumerable.Empty<UnifiedItem>();
            }
            finally
            {
                stopwatch.Stop();
                _metrics.Record(client.Name, stopwatch.ElapsedMilliseconds);
            }
        }

        private IEnumerable<UnifiedItem> ApplyFiltering(
            IEnumerable<UnifiedItem> items,
            AggregationRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                items = items.Where(x =>
                    x.Category != null &&
                    x.Category.Equals(request.Category, StringComparison.OrdinalIgnoreCase));
            }

            return items;
        }

        private IEnumerable<UnifiedItem> ApplySorting(
            IEnumerable<UnifiedItem> items,
            AggregationRequest request)
        {
            return request.SortBy?.ToLower() switch
            {
                "date" => items.OrderByDescending(x => x.Date),
                "title" => items.OrderBy(x => x.Title),
                _ => items
            };
        }
    }
}
