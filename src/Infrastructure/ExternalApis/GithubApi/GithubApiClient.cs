using Application.DTOs;
using Application.Interfaces;
using Core_Infrastructure.ExternalApis.GithubApi.Models;
using Domain.Entities;
using System.Text.Json;

namespace Infrastructure.ExternalApis.GithubApi
{
    public class GithubApiClient : IExternalApiClient
    {
        private readonly HttpClient _http;

        public string Name => "GitHub";

        public GithubApiClient(HttpClient http)
        {
            _http = http;
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("AggregatorApp");
        }

        public async Task<IEnumerable<UnifiedItem>> FetchAsync(AggregationRequest request)
        {
            var url = $"https://api.github.com/search/repositories?q={request.Query}";

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<UnifiedItem>();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<GithubResponse>(json);

            return null;
            //    data.Items.Select(repo => new UnifiedItem
            //{
            //    Source = Name,
            //    Title = repo.Name,
            //    Category = repo.Language ?? "unknown",
            //    Date = repo.UpdatedAt
            //});
        }
    }
}
