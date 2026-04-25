using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalApis;
using Infrastructure.ExternalApis.WeatherApi.Models;
using System.Net.Http.Json;

namespace ApiAggregator.Infrastructure.Clients
{
    public class WeatherApiClient : CachedExternalProviderBase
    {
        private readonly HttpClient _httpClient;

        public WeatherApiClient(HttpClient httpClient, ICacheService cacheService)
            : base(cacheService)
        {
            _httpClient = httpClient;
        }

        public override string ProviderName => "Weather";

        protected override async Task<IEnumerable<UnifiedItem>> ExecuteCoreAsync(string query, CancellationToken cancellationToken)
        {
            var geocoding = await _httpClient.GetFromJsonAsync<OpenMeteoGeocodingResponse>(
                $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(query)}&count=1&language=en&format=json",
                cancellationToken);

            var location = geocoding?.Results?.FirstOrDefault()
                ?? throw new InvalidOperationException($"No weather location found for '{query}'.");

            var forecast = await _httpClient.GetFromJsonAsync<OpenMeteoForecastResponse>(
                $"https://api.open-meteo.com/v1/forecast?latitude={location.Latitude}&longitude={location.Longitude}&current=temperature_2m,weather_code,wind_speed_10m&timezone=UTC",
                cancellationToken)
                ?? throw new InvalidOperationException("Weather forecast response was empty.");

            var current = forecast.Current
                ?? throw new InvalidOperationException("Weather forecast did not include current conditions.");

            return
            [
                new UnifiedItem
                {
                    Source = ProviderName,
                    Title = $"Current weather in {location.Name}, {location.CountryCode}",
                    Description = $"Temperature {current.Temperature} C, wind {current.WindSpeed} km/h, code {current.WeatherCode}.",
                    Category = "Weather",
                    Url = $"https://open-meteo.com/en/docs?city={Uri.EscapeDataString(location.Name)}",
                    RelevanceScore = 0,
                    Date = current.Time
                }
            ];
        }
    }
}
