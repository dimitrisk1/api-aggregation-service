namespace ApiAggregator.Test
{
    using Application.DTOs;
    using Application.Interfaces;
    using Application.Services;
    using FluentAssertions;
    using Domain.Entities;
    using Moq;
    using Xunit;


    public class AggregationServiceTests
    {
        [Fact]
        public async Task AggregateDataAsync_WithPartialFailure_ReturnsCombinedDataAndCorrectStatuses()
        {
            // Arrange
            var mockWeatherClient = new Mock<IExternalApiClient>();
            mockWeatherClient.Setup(c => c.ProviderName).Returns("Weather");
            mockWeatherClient.Setup(c => c.FetchDataAsync<IEnumerable<UnifiedItem>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderResult<IEnumerable<UnifiedItem>>
                {
                    IsSuccess = true,
                    Data = new List<UnifiedItem> { new UnifiedItem { Title = "Sunny" } },
                    Latency = TimeSpan.FromMilliseconds(50)
                });

            var mockGithubClient = new Mock<IExternalApiClient>();
            mockGithubClient.Setup(c => c.ProviderName).Returns("GitHub");
            mockGithubClient.Setup(c => c.FetchDataAsync<IEnumerable<UnifiedItem>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProviderResult<IEnumerable<UnifiedItem>>
                {
                    IsSuccess = false, // Simulating a failure
                    ErrorMessage = "API Rate Limit Exceeded",
                    Latency = TimeSpan.FromMilliseconds(200)
                });

            var mockMetricsService = new Mock<IApiMetricsService>();

            var clients = new List<IExternalApiClient> { mockWeatherClient.Object, mockGithubClient.Object };
            var service = new AggregationService(clients, mockMetricsService.Object);
            var request = new AggregationRequest { Query = "test" };

            // Act
            var result = await service.AggregateDataAsync(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1); 
            result.Items.First().Title.Should().Be("Sunny");

            result.ProviderStatuses["Weather"].Should().Be("Success");
            result.ProviderStatuses["GitHub"].Should().Be("Degraded/Failed");
        }
    }
}