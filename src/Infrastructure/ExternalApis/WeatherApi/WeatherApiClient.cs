namespace Infrastructure.ExternalApis.WeatherApi
{
    using Application.DTOs;
    using Application.Interfaces;
    using Domain.Entities;
    using Microsoft.Extensions.Configuration;
    using System.Text.Json;
    public class WeatherApiClient : IExternalApiClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public string Name => "WeatherAPI";

        public WeatherApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Apis:Weather:ApiKey"];
        }

        public async Task<IEnumerable<UnifiedItem>> FetchAsync(AggregationRequest request)
        {
            if (string.IsNullOrEmpty(request.Query))
                return Enumerable.Empty<UnifiedItem>();

            var url = $"https://api.openweathermap.org/data/2.5/weather?q={request.Query}&appid={_apiKey}&units=metric";

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<UnifiedItem>();

            var json = await response.Content.ReadAsStringAsync();
           // var data = JsonSerializer.Deserialize<WeatherResponse>(json);

            return new List<UnifiedItem>
        {
            new UnifiedItem
            {
                Source = Name,
                //Title = $"Weather in {data.Name}: {data.Main.Temp}°C",
                Category = "weather",
                Date = DateTime.UtcNow
            }
        };
        }
    }
}
