using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.ExternalApis.TwitterApi.Models;
using System.Diagnostics;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients;

public class TwitterApiClient : IExternalProvider
{
    private readonly HttpClient _httpClient;
    public string ProviderName => "Twitter";

    public TwitterApiClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<Domain.Models.ProviderResult<IEnumerable<UnifiedItem>>> FetchDataAsync(string query, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.GetFromJsonAsync<TwitterSearchResponse>($"tweets/search/recent?query={query}", cancellationToken);

            var unifiedItems = response?.Data?.Select(tweet => new UnifiedItem
            {
                Source = ProviderName,                       
                Title = tweet.Text ?? "Empty Tweet",         
                Category = "Social Media",                 
                Date = tweet.CreatedAt                      
            }) ?? Enumerable.Empty<UnifiedItem>();

            return new Domain.Models.ProviderResult<IEnumerable<UnifiedItem>> { IsSuccess = true, Data = unifiedItems, Latency = stopwatch.Elapsed };
        }
        catch (Exception ex)
        {
            return new Domain.Models.ProviderResult<IEnumerable<UnifiedItem>> { IsSuccess = false, ErrorMessage = ex.Message, Latency = stopwatch.Elapsed };
        }
    }



}