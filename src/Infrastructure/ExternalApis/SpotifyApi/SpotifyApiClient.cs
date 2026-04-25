using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalApis.SpootifyApi;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Core_Infrastructure.ExternalApis.SpootifyApi
{
    public class SpotifyClient : IExternalApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string _accessToken;

        public string Name => "Spotify";

        public SpotifyClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _clientId = config["Spotify:ClientId"];
            _clientSecret = config["Spotify:ClientSecret"];
        }

        private async Task AuthenticateAsync()
        {
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

            var response = await _httpClient.SendAsync(request);
            var tokenData = await response.Content.ReadFromJsonAsync<JsonElement>();
            _accessToken = tokenData.GetProperty("access_token").GetString();
        }

     

        public async Task<IEnumerable<UnifiedItem>> FetchAsync(AggregationRequest request)
        {
            if (string.IsNullOrEmpty(_accessToken)) {
                await AuthenticateAsync();
            }

            var req = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(request.Query)}&type=track");

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.SendAsync(req);
                await response.Content.ReadFromJsonAsync<SpotifySearchResponse>();
            return null; 
        }
    }
}
