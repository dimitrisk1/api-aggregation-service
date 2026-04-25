using ApiAggregator.Infrastructure.Clients;
using Application.Common.Configurations;
using Application.Interfaces;
using Application.Services;
using Core_Infrastructure.BackgroundServices;
using Core_Infrastructure.Caching;
using Core_Infrastructure.Services;
using Domain.Interfaces;
using Infrastructure.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core_Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddMemoryCache();
            services.Configure<BackgroundServiceOptions>(config.GetSection("BackgroundService"));

            services.AddScoped<ICacheService, MemoryCacheService>();
            services.AddSingleton<IApiMetricsService, ApiMetricsService>();
            services.AddScoped<IIdentityService, JwtIdentityService>();

            services.AddHttpClient<WeatherApiClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(PollyPolicies.RetryPolicy())
            .AddPolicyHandler(PollyPolicies.CircuitBreakerPolicy())
            .AddPolicyHandler(PollyPolicies.TimeoutPolicy());

            services.AddHttpClient<GitHubApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.github.com/");
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "Core-Api-Aggregator");
            })
            .AddPolicyHandler(PollyPolicies.RetryPolicy())
            .AddPolicyHandler(PollyPolicies.CircuitBreakerPolicy())
            .AddPolicyHandler(PollyPolicies.TimeoutPolicy());

            services.AddHttpClient<StackOverflowApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.stackexchange.com/2.3/");
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(PollyPolicies.RetryPolicy())
            .AddPolicyHandler(PollyPolicies.CircuitBreakerPolicy())
            .AddPolicyHandler(PollyPolicies.TimeoutPolicy());

            services.AddScoped<IExternalProvider>(sp => sp.GetRequiredService<WeatherApiClient>());
            services.AddScoped<IExternalProvider>(sp => sp.GetRequiredService<GitHubApiClient>());
            services.AddScoped<IExternalProvider>(sp => sp.GetRequiredService<StackOverflowApiClient>());

            services.AddHostedService<MetricsMonitorService>();

            return services;
        }
    }
}
