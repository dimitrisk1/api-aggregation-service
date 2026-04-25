using Application.DTOs;
using Core_Infrastructure.ExternalApis.WeatherApi.Models;
using Domain.Entities;
using Domain.Interfaces;
using System.Diagnostics;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients;

public class WeatherApiClient : IExternalProvider
{
    private readonly HttpClient _httpClient;
    public string ProviderName => "Weather";

    public WeatherApiClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<Domain.Models.ProviderResult<IEnumerable<UnifiedItem>>> FetchDataAsync(string query, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            // Assuming the Weather API returns a specific JSON shape
            var response = await _httpClient.GetFromJsonAsync<WeatherApiResponse>($"current.json?q={query}", cancellationToken);

            // Map the external shape to your canonical UnifiedItem
            var unifiedItems = new List<UnifiedItem>
            {
                new UnifiedItem
                {
                    Source = ProviderName,
                    Title = $"Weather in {response?.LocationName}",
                    Category = $"{response?.Temperature}°C, {response?.Condition}",
                    Date = DateTime.UtcNow
                }
            };

            return new Domain.Models.ProviderResult<IEnumerable<UnifiedItem>> { IsSuccess = true, Data = unifiedItems, Latency = stopwatch.Elapsed };
        }
        catch (Exception ex)
        {
            return new Domain.Models.ProviderResult<IEnumerable<UnifiedItem>> { IsSuccess = false, ErrorMessage = ex.Message, Latency = stopwatch.Elapsed };
        }
    }

}