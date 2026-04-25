namespace Infrastructure.ExternalApis.WeatherApi
{
    using Application.DTOs;
    using Application.Interfaces;
    using System.Diagnostics;
    using System.Net.Http.Json;

    public class WeatherApiClient : IExternalApiClient
    {
        private readonly HttpClient _httpClient;

        public string ProviderName => "Weather";

        public WeatherApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Base address and headers are configured in DI
        }

        public async Task<ProviderResult<T>> FetchDataAsync<T>(string query, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // Assuming the external API returns JSON that matches type T
                var response = await _httpClient.GetFromJsonAsync<T>($"weather?q={query}", cancellationToken);

                return new ProviderResult<T>
                {
                    IsSuccess = true,
                    Data = response,
                    Latency = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                // The AggregationService will see IsSuccess = false and apply the partial-failure fallback
                return new ProviderResult<T>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    Latency = stopwatch.Elapsed
                };
            }
        }
    }
}

