namespace Core_Api.Extensions
{
    using Application.Interfaces;
    using Infrastructure.ExternalApis.WeatherApi;
    using Polly;
    using Polly.Extensions.Http;

    public static class HttpClientServiceExtensions
    {
        public static IServiceCollection AddExternalProvidersWithResilience(this IServiceCollection services)
        {
            services.AddHttpClient<IExternalApiClient, WeatherApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.weather.com/v3/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());


            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }
    }
}
