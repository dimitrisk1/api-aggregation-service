using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using System.Diagnostics;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients;

public class SpotifyApiClient : IExternalProvider
{
    private readonly HttpClient _httpClient;
    public string ProviderName => "Spotify";

    public SpotifyApiClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<Domain.Models.ProviderResult<IEnumerable<UnifiedItem>>> FetchDataAsync(string query, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.GetFromJsonAsync<SpotifySearchResponse>($"search?q={query}&type=track", cancellationToken);

            var unifiedItems = response?.Tracks?.Items?.Select(track => new UnifiedItem
            {
                Source = ProviderName,
                Title = track.Name ?? "Unknown Track",
                Category = $"Artist: {track.ArtistName}",
                Date = DateTime.UtcNow // Spotify search might not return a created date, so we default to now
            }) ?? Enumerable.Empty<UnifiedItem>();

            return new Domain.Models.ProviderResult<IEnumerable<UnifiedItem>> { IsSuccess = true, Data = unifiedItems, Latency = stopwatch.Elapsed };
        }
        catch (Exception ex)
        {
            return new Domain.Models.ProviderResult<IEnumerable<UnifiedItem>> { IsSuccess = false, ErrorMessage = ex.Message, Latency = stopwatch.Elapsed };
        }
    }

}