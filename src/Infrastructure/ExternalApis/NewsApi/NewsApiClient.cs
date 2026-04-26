using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalApis;
using Infrastructure.ExternalApis.NewsApi.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients
{
    public class NewsApiClient : CachedExternalProviderBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public NewsApiClient(HttpClient httpClient, ICacheService cacheService, IConfiguration configuration)
            : base(cacheService)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Apis:News:ApiKey"] ?? string.Empty;
        }

        public override string ProviderName => "News";

        protected override async Task<IEnumerable<UnifiedItem>> ExecuteCoreAsync(string query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.StartsWith("replace-with-", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("News API key is missing. Set Apis:News:ApiKey in configuration.");
            }

            using var response = await _httpClient.GetAsync(
                $"everything?q={Uri.EscapeDataString(query)}&language=en&pageSize=10&sortBy=publishedAt&apiKey={Uri.EscapeDataString(_apiKey)}",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<NewsApiResponse>(cancellationToken: cancellationToken)
                ?? new NewsApiResponse();

            return payload.Articles?.Select(article => new UnifiedItem
            {
                Source = ProviderName,
                Title = article.Title ?? "Untitled article",
                Description = article.Description ?? article.Content ?? "No description provided.",
                Category = article.Source?.Name ?? "News",
                Url = article.Url ?? string.Empty,
                RelevanceScore = article.PublishedAt == default
                    ? 0
                    : Math.Max(0, 1000000 - (DateTime.UtcNow - article.PublishedAt.ToUniversalTime()).TotalMinutes),
                Date = article.PublishedAt == default ? DateTime.UtcNow : article.PublishedAt.ToUniversalTime()
            }) ?? Enumerable.Empty<UnifiedItem>();
        }
    }
}
