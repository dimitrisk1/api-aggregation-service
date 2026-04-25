using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalApis.TwitterApi.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Infrastructure.ExternalApis.TwitterApi
{
    public class TwitterClient : IExternalApiClient
    {
        private readonly HttpClient _httpClient;
        public string Name => "Twitter";

        public TwitterClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            // Setup Base Address and Headers
            _httpClient.BaseAddress = new Uri("https://api.twitter.com/2/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config["Twitter:BearerToken"]}");
        }


        public async Task<IEnumerable<UnifiedItem>> FetchAsync(AggregationRequest request)
        {
            var response = await _httpClient.GetAsync($"tweets/search/recent?query={Uri.EscapeDataString(request.Query)}");

            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<TweetResponse>();
            return null;
        }  
    }
}
