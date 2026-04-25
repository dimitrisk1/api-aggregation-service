using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalApis;
using Infrastructure.ExternalApis.StackOverflowApi.Models;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients
{
    public class StackOverflowApiClient : CachedExternalProviderBase
    {
        private readonly HttpClient _httpClient;

        public StackOverflowApiClient(HttpClient httpClient, ICacheService cacheService)
            : base(cacheService)
        {
            _httpClient = httpClient;
        }

        public override string ProviderName => "StackOverflow";

        protected override async Task<IEnumerable<UnifiedItem>> ExecuteCoreAsync(string query, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(
                $"search/advanced?order=desc&sort=activity&q={Uri.EscapeDataString(query)}&site=stackoverflow&pagesize=10",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<StackOverflowResponse>(cancellationToken: cancellationToken)
                ?? new StackOverflowResponse();

            return payload.Items?.Select(question => new UnifiedItem
            {
                Source = ProviderName,
                Title = question.Title ?? "Untitled question",
                Description = $"Answered: {question.IsAnswered}. Tags: {string.Join(", ", question.Tags ?? [])}",
                Category = question.Tags?.FirstOrDefault() ?? "Question",
                Url = question.Link ?? string.Empty,
                RelevanceScore = question.Score,
                Date = DateTimeOffset.FromUnixTimeSeconds(question.LastActivityDate).UtcDateTime
            }) ?? Enumerable.Empty<UnifiedItem>();
        }
    }
}
