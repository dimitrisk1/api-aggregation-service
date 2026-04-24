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
        public async Task Should_Aggregate_Data_From_All_Apis()
        {
            var mockClients = new List<IExternalApiClient>
        {
            CreateMockClient("NewsAPI", new List<UnifiedItem>
            {
                new UnifiedItem { Title = "News1", Source = "NewsAPI" }
            }),
            CreateMockClient("WeatherAPI", new List<UnifiedItem>
            {
                new UnifiedItem { Title = "Weather1", Source = "WeatherAPI" }
            }),
            CreateMockClient("GitHub", new List<UnifiedItem>
            {
                new UnifiedItem { Title = "Repo1", Source = "GitHub" }
            })
        };

            var metricsMock = new Mock<IApiMetricsService>();

            var service = new AggregationService(mockClients, metricsMock.Object);

            var request = new AggregationRequest
            {
                Query = "test"
            };

            var result = await service.HandleAsync(request);

            result.Items.Should().HaveCount(3);
            result.Items.Select(x => x.Source).Should().Contain(new[]
            {
            "NewsAPI", "WeatherAPI", "GitHub"
        });
        }

        [Fact]
        public async Task Should_Return_Partial_Data_When_One_Api_Fails()
        {
            var workingClient = CreateMockClient("NewsAPI", new List<UnifiedItem>
        {
            new UnifiedItem { Title = "News1", Source = "NewsAPI" }
        });

            var failingClient = new Mock<IExternalApiClient>();
            failingClient.Setup(c => c.Name).Returns("WeatherAPI");
            failingClient
                .Setup(c => c.FetchAsync(It.IsAny<AggregationRequest>()))
                .ThrowsAsync(new Exception("API failed"));

            var clients = new List<IExternalApiClient>
        {
            workingClient,
            failingClient.Object
        };

            var metricsMock = new Mock<IApiMetricsService>();

            var service = new AggregationService(clients, metricsMock.Object);

            var result = await service.HandleAsync(new AggregationRequest());

            result.Items.Should().HaveCount(1);
            result.Items.First().Source.Should().Be("NewsAPI");
        }

        [Fact]
        public async Task Should_Record_Metrics_For_Each_Client()
        {
            var client = CreateMockClient("NewsAPI", new List<UnifiedItem>
        {
            new UnifiedItem { Title = "News1", Source = "NewsAPI" }
        });

            var metricsMock = new Mock<IApiMetricsService>();

            var service = new AggregationService(
                new List<IExternalApiClient> { client },
                metricsMock.Object);

            // Act
            await service.HandleAsync(new AggregationRequest());

            // Assert
            metricsMock.Verify(
                m => m.Record("NewsAPI", It.IsAny<long>()),
                Times.Once);
        }

        private IExternalApiClient CreateMockClient(string name, List<UnifiedItem> data)
        {
            var mock = new Mock<IExternalApiClient>();

            mock.Setup(c => c.Name).Returns(name);
            mock.Setup(c => c.FetchAsync(It.IsAny<AggregationRequest>()))
                .ReturnsAsync(data);

            return mock.Object;
        }
    }
}