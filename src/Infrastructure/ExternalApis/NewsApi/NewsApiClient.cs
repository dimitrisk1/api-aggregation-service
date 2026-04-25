using Application.DTOs;
using Application.Interfaces;
using Core_Infrastructure.ExternalApis.NewsApi.Models;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Infrastructure.ExternalApis.NewsApi
{
    public class NewsApiClient : IExternalApiClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public string Name => "NewsAPI";

        public NewsApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Apis:News:ApiKey"];
        }

        public async Task<IEnumerable<UnifiedItem>> FetchAsync(AggregationRequest request)
        {
            var url = $"https://newsapi.org/v2/everything?q={request.Query}&apiKey={_apiKey}";

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<UnifiedItem>();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<NewsApiResponse>(json);

            return
                data.Articles.Select(a => new UnifiedItem
                {
                    Source = Name,
                    Title = a.Title,
                    Category = a.Source.Name,
                    Date = a.PublishedAt
                });
        }
    }
}
