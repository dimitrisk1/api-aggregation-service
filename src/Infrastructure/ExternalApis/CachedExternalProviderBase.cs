using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Models;
using System.Diagnostics;

namespace Infrastructure.ExternalApis
{
    public abstract class CachedExternalProviderBase : IExternalProvider
    {
        private readonly ICacheService _cacheService;

        protected CachedExternalProviderBase(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public abstract string ProviderName { get; }

        protected virtual TimeSpan CacheDuration => TimeSpan.FromMinutes(5);

        public async Task<ProviderResult<IEnumerable<UnifiedItem>>> FetchDataAsync(string query, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var cacheKey = $"{ProviderName}:{query.Trim().ToLowerInvariant()}";

            try
            {
                var items = (await ExecuteCoreAsync(query, cancellationToken)).ToList();
                _cacheService.Set(cacheKey, items, CacheDuration);

                return new ProviderResult<IEnumerable<UnifiedItem>>
                {
                    IsSuccess = true,
                    Data = items,
                    Latency = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                if (_cacheService.TryGetValue<List<UnifiedItem>>(cacheKey, out var fallbackItems) && fallbackItems is not null)
                {
                    return new ProviderResult<IEnumerable<UnifiedItem>>
                    {
                        IsSuccess = true,
                        IsFallback = true,
                        Data = fallbackItems,
                        ErrorMessage = ex.Message,
                        Latency = stopwatch.Elapsed
                    };
                }

                return new ProviderResult<IEnumerable<UnifiedItem>>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    Latency = stopwatch.Elapsed
                };
            }
        }

        protected abstract Task<IEnumerable<UnifiedItem>> ExecuteCoreAsync(string query, CancellationToken cancellationToken);
    }
}
