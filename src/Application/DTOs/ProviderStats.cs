namespace Application.DTOs
{
    public record ProviderStats(
        long TotalRequests,
        TimeSpan LifetimeAverage,
        TimeSpan LastFiveMinutesAverage,
        int RecentSampleCount);
}
