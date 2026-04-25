using Application.DTOs;
using Application.Interfaces;
using System.Collections.Concurrent;

namespace Application.Services
{
    using Application.Models;
    // Infrastructure/Metrics/ApiMetricsService.cs
    using System.Collections.Concurrent;

    public class ApiMetricsService : IApiMetricsService
    {
        // A thread-safe dictionary holding the state for each provider
        private readonly ConcurrentDictionary<string, ProviderMetricsState> _metrics = new();

        public void RecordMetric(string providerName, TimeSpan latency, bool isSuccess)
        {
            var state = _metrics.GetOrAdd(providerName, _ => new ProviderMetricsState());
            state.Record(latency, isSuccess);
        }

        public ProviderStats GetProviderStats(string providerName)
        {
            if (_metrics.TryGetValue(providerName, out var state))
            {
                return state.GetStats();
            }
            return new ProviderStats(0, TimeSpan.Zero, TimeSpan.Zero, 0);
        }

        // Nested class to encapsulate the thread-safe logic per provider
        private class ProviderMetricsState
        {
            private long _totalRequests;
            private long _totalTicks; // TimeSpan is backed by ticks

            // Tracks only the last 5 minutes of requests
            private readonly ConcurrentQueue<TimingSample> _recentSamples = new();

            public void Record(TimeSpan latency, bool isSuccess)
            {
                // Thread-safe lifetime updates
                Interlocked.Increment(ref _totalRequests);
                Interlocked.Add(ref _totalTicks, latency.Ticks);

                // Add to rolling window
                _recentSamples.Enqueue(new TimingSample(DateTime.UtcNow, latency, isSuccess));

                // Prevent memory leaks by trimming old samples
                TrimOldSamples();
            }

            public ProviderStats GetStats()
            {
                long totalRequests = Interlocked.Read(ref _totalRequests);
                long totalTicks = Interlocked.Read(ref _totalTicks);

                var lifetimeAvg = totalRequests == 0 ? TimeSpan.Zero : TimeSpan.FromTicks(totalTicks / totalRequests);

                // Calculate rolling 5-minute average
                var recentSamplesSnapshot = _recentSamples.ToArray();
                var recentCount = recentSamplesSnapshot.Length;
                var recentAvg = recentCount == 0
                    ? TimeSpan.Zero
                    : TimeSpan.FromTicks((long)recentSamplesSnapshot.Average(s => s.Latency.Ticks));

                return new ProviderStats(totalRequests, lifetimeAvg, recentAvg, recentCount);
            }

            private void TrimOldSamples()
            {
                var cutoff = DateTime.UtcNow.AddMinutes(-5);

                // Look at the oldest item. If it's older than 5 mins, try to dequeue it.
                while (_recentSamples.TryPeek(out var oldest) && oldest.Timestamp < cutoff)
                {
                    _recentSamples.TryDequeue(out _);
                }
            }
        }
    }
}


