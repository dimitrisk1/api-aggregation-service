namespace Application.DTOs
{
    public record ProviderStats(
        long TotalRequests,
        long SuccessfulRequests,
        long FailedRequests,
        TimeSpan LifetimeAverage,
        TimeSpan LastFiveMinutesAverage,
        int RecentSampleCount,
        IReadOnlyDictionary<string, long> Buckets);
}
