using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalApis;
using Infrastructure.ExternalApis.WeatherApi.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients
{
    public class WeatherApiClient : CachedExternalProviderBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherApiClient(HttpClient httpClient, ICacheService cacheService, IConfiguration configuration)
            : base(cacheService)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Apis:Weather:ApiKey"] ?? string.Empty;
        }

        public override string ProviderName => "Weather";

        protected override async Task<IEnumerable<UnifiedItem>> ExecuteCoreAsync(string query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.StartsWith("replace-with-", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Weather API key is missing. Set Apis:Weather:ApiKey in configuration.");
            }

            using var response = await _httpClient.GetAsync(
                $"weather?q={Uri.EscapeDataString(query)}&appid={Uri.EscapeDataString(_apiKey)}&units=metric",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<OpenWeatherResponse>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Weather API response was empty.");

            var currentCondition = payload.Weather?.FirstOrDefault();
            var observedAt = payload.Timestamp > 0
                ? DateTimeOffset.FromUnixTimeSeconds(payload.Timestamp).UtcDateTime
                : DateTime.UtcNow;

            return
            [
                new UnifiedItem
                {
                    Source = ProviderName,
                    Title = $"Current weather in {payload.Name}",
                    Description = $"Temperature {payload.Main?.Temperature ?? 0} C, humidity {payload.Main?.Humidity ?? 0}%, condition {currentCondition?.Description ?? currentCondition?.Main ?? "Unknown"}.",
                    Category = "Weather",
                    Url = $"https://openweathermap.org/city/{payload.CityId}",
                    RelevanceScore = 0,
                    Date = observedAt
                }
            ];
        }
    }
}
