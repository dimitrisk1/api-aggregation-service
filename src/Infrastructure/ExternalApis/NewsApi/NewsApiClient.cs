using Application.DTOs;
using Core_Infrastructure.ExternalApis.NewsApi.Models;
using Domain.Entities;
using Domain.Interfaces;
using System.Diagnostics;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients;

public class NewsApiClient : IExternalProvider
{
    private readonly HttpClient _httpClient;
    public string ProviderName => "News";

    public NewsApiClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<Domain.Models.ProviderResult<IEnumerable<UnifiedItem>>> FetchDataAsync(string query, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.GetFromJsonAsync<NewsApiResponse>($"everything?q={query}", cancellationToken);

            var unifiedItems = response?.Articles?.Select(article => new UnifiedItem
            {
                Source = ProviderName,
                Title = article.Headline ?? "No Title",
                Category = article.Summary ?? "No Summary",
                Date = article.PublishedAt
            }) ?? Enumerable.Empty<UnifiedItem>();

            return new Domain.Models.ProviderResult<IEnumerable<UnifiedItem>> { IsSuccess = true, Data = unifiedItems, Latency = stopwatch.Elapsed };
        }
        catch (Exception ex)
        {
            return new Domain.Models.ProviderResult<IEnumerable<UnifiedItem>> { IsSuccess = false, ErrorMessage = ex.Message, Latency = stopwatch.Elapsed };
        }
    }

}