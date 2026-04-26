using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace ApiAggregator.Test
{
    public class AggregationServiceTests
    {
        [Fact]
        public async Task AggregateDataAsync_WithPartialFailureAndFallback_ReturnsCombinedDataAndProviderStatuses()
        {
            var weatherProvider = CreateProvider(
                "Weather",
                new ProviderResult<IEnumerable<UnifiedItem>>
                {
                    IsSuccess = true,
                    Data =
                    [
                        new UnifiedItem
                        {
                            Source = "Weather",
                            Title = "Current weather in London",
                            Description = "Cloudy",
                            Category = "Weather",
                            Date = new DateTime(2026, 4, 25, 10, 0, 0, DateTimeKind.Utc),
                            RelevanceScore = 0
                        }
                    ],
                    Latency = TimeSpan.FromMilliseconds(80)
                });

            var githubProvider = CreateProvider(
                "GitHub",
                new ProviderResult<IEnumerable<UnifiedItem>>
                {
                    IsSuccess = true,
                    IsFallback = true,
                    Data =
                    [
                        new UnifiedItem
                        {
                            Source = "GitHub",
                            Title = "dotnet/runtime",
                            Description = "Core runtime",
                            Category = "C#",
                            Date = new DateTime(2026, 4, 24, 10, 0, 0, DateTimeKind.Utc),
                            RelevanceScore = 5000
                        }
                    ],
                    ErrorMessage = "Primary endpoint unavailable.",
                    Latency = TimeSpan.FromMilliseconds(220)
                });

            var stackOverflowProvider = CreateProvider(
                "StackOverflow",
                new ProviderResult<IEnumerable<UnifiedItem>>
                {
                    IsSuccess = false,
                    ErrorMessage = "503 Service Unavailable",
                    Latency = TimeSpan.FromMilliseconds(350)
                });

            var metricsService = new Mock<IApiMetricsService>();
            var service = new AggregationService(
                [weatherProvider.Object, githubProvider.Object, stackOverflowProvider.Object],
                metricsService.Object);

            var result = await service.AggregateDataAsync(new AggregationRequest { Query = "dotnet" }, CancellationToken.None);

            result.TotalItems.Should().Be(2);
            result.Items.Should().ContainSingle(item => item.Source == "Weather");
            result.Items.Should().ContainSingle(item => item.Source == "GitHub");
            result.Providers.Should().ContainSingle(provider => provider.ProviderName == "Weather" && provider.Status == "Success");
            result.Providers.Should().ContainSingle(provider => provider.ProviderName == "GitHub" && provider.Status == "Fallback");
            result.Providers.Should().ContainSingle(provider => provider.ProviderName == "StackOverflow" && provider.Status == "Failed");

            metricsService.Verify(service => service.RecordMetric("Weather", TimeSpan.FromMilliseconds(80), true), Times.Once);
            metricsService.Verify(service => service.RecordMetric("GitHub", TimeSpan.FromMilliseconds(220), false), Times.Once);
            metricsService.Verify(service => service.RecordMetric("StackOverflow", TimeSpan.FromMilliseconds(350), false), Times.Once);
        }

        [Fact]
        public async Task AggregateDataAsync_AppliesFiltersAndSortsByRelevance()
        {
            var provider = CreateProvider(
                "GitHub",
                new ProviderResult<IEnumerable<UnifiedItem>>
                {
                    IsSuccess = true,
                    Data =
                    [
                        new UnifiedItem
                        {
                            Source = "GitHub",
                            Title = "repo-low",
                            Description = "desc",
                            Category = "C#",
                            Date = new DateTime(2026, 4, 23, 10, 0, 0, DateTimeKind.Utc),
                            RelevanceScore = 10
                        },
                        new UnifiedItem
                        {
                            Source = "GitHub",
                            Title = "repo-high",
                            Description = "desc",
                            Category = "C#",
                            Date = new DateTime(2026, 4, 25, 10, 0, 0, DateTimeKind.Utc),
                            RelevanceScore = 100
                        },
                        new UnifiedItem
                        {
                            Source = "Weather",
                            Title = "weather",
                            Description = "desc",
                            Category = "Weather",
                            Date = new DateTime(2026, 4, 25, 9, 0, 0, DateTimeKind.Utc),
                            RelevanceScore = 0
                        }
                    ],
                    Latency = TimeSpan.FromMilliseconds(80)
                });

            var service = new AggregationService([provider.Object], Mock.Of<IApiMetricsService>());

            var result = await service.AggregateDataAsync(
                new AggregationRequest
                {
                    Query = "dotnet",
                    Source = "GitHub",
                    Category = "C#",
                    SortBy = "relevance",
                    SortDirection = "desc",
                    Limit = 1
                },
                CancellationToken.None);

            result.TotalItems.Should().Be(1);
            result.Items[0].Title.Should().Be("repo-high");
        }

        private static Mock<IExternalProvider> CreateProvider(string providerName, ProviderResult<IEnumerable<UnifiedItem>> result)
        {
            var provider = new Mock<IExternalProvider>();
            provider.SetupGet(x => x.ProviderName).Returns(providerName);
            provider.Setup(x => x.FetchDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
            return provider;
        }
    }
}
