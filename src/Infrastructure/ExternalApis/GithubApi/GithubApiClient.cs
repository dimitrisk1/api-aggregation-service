using Application.DTOs;
using Core_Infrastructure.ExternalApis.GithubApi.Models;
using Domain.Entities;
using Domain.Interfaces;
using System.Diagnostics;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients;

public class GitHubApiClient : IExternalProvider
{
    private readonly HttpClient _httpClient;
    public string ProviderName => "GitHub";

    public GitHubApiClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<Domain.Models.ProviderResult<IEnumerable<UnifiedItem>>> FetchDataAsync(string query, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.GetFromJsonAsync<GithubSearchResponse>($"search/repositories?q={query}", cancellationToken);

            var unifiedItems = response?.Items?.Select(repo => new UnifiedItem
            {
                Source = ProviderName,
                Title = repo.FullName ?? "Unknown Repo",
                Category = repo.Description ?? "No description",
                Date = repo.UpdatedAt
            }) ?? Enumerable.Empty<UnifiedItem>();

            return new Domain.Models.ProviderResult<IEnumerable<UnifiedItem>> { IsSuccess = true, Data = unifiedItems, Latency = stopwatch.Elapsed };
        }
        catch (Exception ex)
        {
            return new Domain.Models.ProviderResult<IEnumerable<UnifiedItem>> { IsSuccess = false, ErrorMessage = ex.Message, Latency = stopwatch.Elapsed };
        }
    }

}