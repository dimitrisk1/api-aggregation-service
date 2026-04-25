using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Core_Infrastructure.Caching
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool TryGetValue<T>(string key, out T? value)
        {
            if (_cache.TryGetValue(key, out var cached) && cached is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
        {
            _cache.Set(
                key,
                value,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
                });
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)
        {
            if (TryGetValue<T>(key, out var cached) && cached is not null)
            {
                return cached;
            }

            var value = await factory();
            Set(key, value, TimeSpan.FromMinutes(5));
            return value;
        }
    }
}
