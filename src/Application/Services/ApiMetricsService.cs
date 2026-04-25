using Application.DTOs;
using Application.Interfaces;
using Application.Models;
using System.Collections.Concurrent;

namespace Application.Services
{
    public class ApiMetricsService : IApiMetricsService
    {
        private readonly ConcurrentDictionary<string, ProviderMetricsState> _metrics = new();

        private static readonly (string Name, Func<TimeSpan, bool> Match)[] BucketDefinitions =
        [
            ("fast", latency => latency.TotalMilliseconds < 100),
            ("average", latency => latency.TotalMilliseconds >= 100 && latency.TotalMilliseconds <= 250),
            ("slow", latency => latency.TotalMilliseconds > 250)
        ];

        public void RecordMetric(string providerName, TimeSpan latency, bool isSuccess)
        {
            var state = _metrics.GetOrAdd(providerName, _ => new ProviderMetricsState());
            state.Record(latency, isSuccess);
        }

        public ApiStatsDto GetApiStatistics()
        {
            return new ApiStatsDto
            {
                Providers = _metrics.ToDictionary(
                    entry => entry.Key,
                    entry =>
                    {
                        var stats = entry.Value.GetStats();
                        return new ApiStatItemDto
                        {
                            TotalRequests = stats.TotalRequests,
                            SuccessfulRequests = stats.SuccessfulRequests,
                            FailedRequests = stats.FailedRequests,
                            AverageResponseTimeMs = Math.Round(stats.LifetimeAverage.TotalMilliseconds, 2),
                            LastFiveMinutesAverageResponseTimeMs = Math.Round(stats.LastFiveMinutesAverage.TotalMilliseconds, 2),
                            Buckets = stats.Buckets.ToDictionary(bucket => bucket.Key, bucket => bucket.Value)
                        };
                    })
            };
        }

        public ProviderStats GetProviderStats(string providerName)
        {
            return _metrics.TryGetValue(providerName, out var state)
                ? state.GetStats()
                : new ProviderStats(0, 0, 0, TimeSpan.Zero, TimeSpan.Zero, 0, CreateEmptyBuckets());
        }

        public IReadOnlyDictionary<string, ProviderStats> GetStats()
        {
            return _metrics.ToDictionary(entry => entry.Key, entry => entry.Value.GetStats());
        }

        private sealed class ProviderMetricsState
        {
            private readonly ConcurrentQueue<TimingSample> _recentSamples = new();
            private readonly ConcurrentDictionary<string, long> _bucketCounts = new(
                BucketDefinitions.ToDictionary(bucket => bucket.Name, _ => 0L));

            private long _totalRequests;
            private long _totalTicks;
            private long _successfulRequests;
            private long _failedRequests;

            public void Record(TimeSpan latency, bool isSuccess)
            {
                Interlocked.Increment(ref _totalRequests);
                Interlocked.Add(ref _totalTicks, latency.Ticks);

                if (isSuccess)
                {
                    Interlocked.Increment(ref _successfulRequests);
                }
                else
                {
                    Interlocked.Increment(ref _failedRequests);
                }

                var bucketName = BucketDefinitions.First(bucket => bucket.Match(latency)).Name;
                _bucketCounts.AddOrUpdate(bucketName, 1, (_, current) => current + 1);

                _recentSamples.Enqueue(new TimingSample(DateTime.UtcNow, latency, isSuccess));
                TrimOldSamples();
            }

            public ProviderStats GetStats()
            {
                var totalRequests = Interlocked.Read(ref _totalRequests);
                var totalTicks = Interlocked.Read(ref _totalTicks);
                var recentSamples = _recentSamples.ToArray();
                var recentCount = recentSamples.Length;

                return new ProviderStats(
                    totalRequests,
                    Interlocked.Read(ref _successfulRequests),
                    Interlocked.Read(ref _failedRequests),
                    totalRequests == 0 ? TimeSpan.Zero : TimeSpan.FromTicks(totalTicks / totalRequests),
                    recentCount == 0
                        ? TimeSpan.Zero
                        : TimeSpan.FromTicks((long)recentSamples.Average(sample => sample.Latency.Ticks)),
                    recentCount,
                    _bucketCounts.ToDictionary(entry => entry.Key, entry => entry.Value));
            }

            private void TrimOldSamples()
            {
                var cutoff = DateTime.UtcNow.AddMinutes(-5);
                while (_recentSamples.TryPeek(out var oldest) && oldest.Timestamp < cutoff)
                {
                    _recentSamples.TryDequeue(out _);
                }
            }
        }

        private static IReadOnlyDictionary<string, long> CreateEmptyBuckets()
        {
            return BucketDefinitions.ToDictionary(bucket => bucket.Name, _ => 0L);
        }
    }
}
