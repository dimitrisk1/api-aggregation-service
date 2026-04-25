using Application.Services;
using FluentAssertions;

namespace ApiAggregator.Test
{
    public class ApiMetricsServiceTests
    {
        [Fact]
        public void GetApiStatistics_GroupsRequestsIntoBucketsAndCalculatesAverages()
        {
            var service = new ApiMetricsService();

            service.RecordMetric("GitHub", TimeSpan.FromMilliseconds(50), true);
            service.RecordMetric("GitHub", TimeSpan.FromMilliseconds(150), true);
            service.RecordMetric("GitHub", TimeSpan.FromMilliseconds(400), false);

            var stats = service.GetApiStatistics();

            stats.Providers.Should().ContainKey("GitHub");
            stats.Providers["GitHub"].TotalRequests.Should().Be(3);
            stats.Providers["GitHub"].SuccessfulRequests.Should().Be(2);
            stats.Providers["GitHub"].FailedRequests.Should().Be(1);
            stats.Providers["GitHub"].Buckets["fast"].Should().Be(1);
            stats.Providers["GitHub"].Buckets["average"].Should().Be(1);
            stats.Providers["GitHub"].Buckets["slow"].Should().Be(1);
            stats.Providers["GitHub"].AverageResponseTimeMs.Should().BeApproximately(200, 0.01);
        }
    }
}
