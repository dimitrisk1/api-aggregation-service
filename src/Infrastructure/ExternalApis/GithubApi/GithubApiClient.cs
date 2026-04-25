using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalApis;
using Infrastructure.ExternalApis.GithubApi.Models;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients
{
    public class GitHubApiClient : CachedExternalProviderBase
    {
        private readonly HttpClient _httpClient;

        public GitHubApiClient(HttpClient httpClient, ICacheService cacheService)
            : base(cacheService)
        {
            _httpClient = httpClient;
        }

        public override string ProviderName => "GitHub";

        protected override async Task<IEnumerable<UnifiedItem>> ExecuteCoreAsync(string query, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(
                $"search/repositories?q={Uri.EscapeDataString(query)}&sort=updated&order=desc&per_page=10",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<GithubSearchResponse>(cancellationToken: cancellationToken)
                ?? new GithubSearchResponse();

            return payload.Items?.Select(repo => new UnifiedItem
            {
                Source = ProviderName,
                Title = repo.FullName ?? "Unknown repository",
                Description = repo.Description ?? "No description provided.",
                Category = string.IsNullOrWhiteSpace(repo.Language) ? "Repository" : repo.Language,
                Url = repo.HtmlUrl ?? string.Empty,
                RelevanceScore = repo.Stars,
                Date = repo.UpdatedAt
            }) ?? Enumerable.Empty<UnifiedItem>();
        }
    }
}
