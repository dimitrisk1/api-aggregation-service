using Application.Interfaces;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.ExternalApis;

namespace ApiAggregator.Test
{
    public class CachedExternalProviderBaseTests
    {
        [Fact]
        public async Task FetchDataAsync_ReturnsCachedItems_WithoutCallingUpstream()
        {
            var cachedItems = new List<UnifiedItem>
            {
                new()
                {
                    Source = "Test",
                    Title = "cached"
                }
            };

            var cache = new FakeCacheService();
            cache.Set("Test:dotnet", cachedItems, TimeSpan.FromMinutes(5));

            var provider = new TestCachedProvider(cache)
            {
                ExecuteCoreAsyncImpl = (_, _) => throw new InvalidOperationException("Upstream should not be called.")
            };

            var result = await provider.FetchDataAsync("dotnet", CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.IsFallback.Should().BeFalse();
            result.Data.Should().ContainSingle(item => item.Title == "cached");
        }

        [Fact]
        public async Task FetchDataAsync_ReturnsFallbackItems_WhenUpstreamFails()
        {
            var fallbackItems = new List<UnifiedItem>
            {
                new()
                {
                    Source = "Test",
                    Title = "fallback"
                }
            };
            var cache = new FakeCacheService
            {
                ReadsBeforeHit = 1
            };
            cache.Set("Test:dotnet", fallbackItems, TimeSpan.FromMinutes(5));

            var provider = new TestCachedProvider(cache)
            {
                ExecuteCoreAsyncImpl = (_, _) => throw new HttpRequestException("boom")
            };

            var result = await provider.FetchDataAsync("dotnet", CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.IsFallback.Should().BeTrue();
            result.Data.Should().ContainSingle(item => item.Title == "fallback");
        }

        [Fact]
        public async Task FetchDataAsync_PropagatesCancellation()
        {
            var cache = new FakeCacheService();

            var provider = new TestCachedProvider(cache)
            {
                ExecuteCoreAsyncImpl = (_, cancellationToken) => throw new OperationCanceledException(cancellationToken)
            };

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var act = () => provider.FetchDataAsync("dotnet", cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        private sealed class FakeCacheService : ICacheService
        {
            private readonly Dictionary<string, object> _values = new();
            private int _readCount;

            public int ReadsBeforeHit { get; set; }

            public bool TryGetValue<T>(string key, out T? value)
            {
                _readCount++;

                if (_readCount > ReadsBeforeHit &&
                    _values.TryGetValue(key, out var rawValue) &&
                    rawValue is T typedValue)
                {
                    value = typedValue;
                    return true;
                }

                value = default;
                return false;
            }

            public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow)
            {
                _values[key] = value!;
            }

            public Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class TestCachedProvider : CachedExternalProviderBase
        {
            public TestCachedProvider(ICacheService cacheService)
                : base(cacheService)
            {
            }

            public override string ProviderName => "Test";

            public Func<string, CancellationToken, Task<IEnumerable<UnifiedItem>>> ExecuteCoreAsyncImpl { get; set; } =
                (_, _) => Task.FromResult<IEnumerable<UnifiedItem>>(Array.Empty<UnifiedItem>());

            protected override Task<IEnumerable<UnifiedItem>> ExecuteCoreAsync(string query, CancellationToken cancellationToken)
            {
                return ExecuteCoreAsyncImpl(query, cancellationToken);
            }
        }
    }
}
