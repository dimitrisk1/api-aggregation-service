using Application.DTOs;
using Application.Interfaces;
using System.Collections.Concurrent;

namespace Application.Services
{
    public class ApiMetricsService : IApiMetricsService
    {
        private readonly ConcurrentDictionary<string, ApiStatItemDto> _stats = new();

        public void Record(string apiName, long time)
        {
            var stat = _stats.GetOrAdd(apiName, _ => new ApiStatItemDto());

            //Interlocked.Increment(ref stat.TotalRequests);
            //Interlocked.Add(ref stat.TotalResponseTime, time);

            var bucket = time < 100 ? "fast" :
                         time < 200 ? "average" : "slow";

           // stat.Buckets.Add("fast", 1, (_, v) => v + 1);
        }

        public ApiStatsDto GetStats()
        {
            var result = _stats.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    var stat = kvp.Value;

                    var avg = stat.TotalRequests == 0
                        ? 0
                        : (double)stat.TotalResponseTime / stat.TotalRequests;

                    return new ApiStatItemDto
                    {
                        TotalRequests = stat.TotalRequests,
                        TotalResponseTime = stat.TotalResponseTime,
                        Buckets = new Dictionary<string, int>(stat.Buckets)
                    };
                });

            return new ApiStatsDto
            {
                APIs = result
            };
        }
    }
}

