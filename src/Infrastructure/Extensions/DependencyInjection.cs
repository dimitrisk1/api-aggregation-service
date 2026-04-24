using Application.Interfaces;
using Application.Services;
using Infrastructure.ExternalApis.GithubApi;
using Infrastructure.ExternalApis.NewsApi;
using Infrastructure.ExternalApis.WeatherApi;
using Infrastructure.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Core_Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration config)
        {
            //services.AddMemoryCache();

            //services.AddScoped<ICacheService, MemoryCacheService>();
            services.AddSingleton<IApiMetricsService, ApiMetricsService>();

            services.AddHttpClient<NewsApiClient>()
                 .AddPolicyHandler(PollyPolicies.RetryPolicy())
                 .AddPolicyHandler(PollyPolicies.CircuitBreakerPolicy())
                 .AddPolicyHandler(PollyPolicies.TimeoutPolicy());

            services.AddHttpClient<WeatherApiClient>()
                .AddPolicyHandler(PollyPolicies.RetryPolicy())
                .AddPolicyHandler(PollyPolicies.TimeoutPolicy());

            services.AddHttpClient<GithubApiClient>()
                .AddPolicyHandler(PollyPolicies.RetryPolicy());

            services.AddScoped<IExternalApiClient, NewsApiClient>();
            services.AddScoped<IExternalApiClient, WeatherApiClient>();
            services.AddScoped<IExternalApiClient, GithubApiClient>();

            return services;
        }
    }
}
