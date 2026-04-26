using Application.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalApis;
using Infrastructure.ExternalApis.SpotifyApi.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace ApiAggregator.Infrastructure.Clients
{
    public class SpotifyApiClient : CachedExternalProviderBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly SemaphoreSlim _tokenLock = new(1, 1);
        private string? _accessToken;
        private DateTimeOffset _expiresAtUtc = DateTimeOffset.MinValue;

        public SpotifyApiClient(HttpClient httpClient, ICacheService cacheService, IConfiguration configuration)
            : base(cacheService)
        {
            _httpClient = httpClient;
            _clientId = configuration["Apis:Spotify:ClientId"] ?? string.Empty;
            _clientSecret = configuration["Apis:Spotify:ClientSecret"] ?? string.Empty;
        }

        public override string ProviderName => "Spotify";

        protected override async Task<IEnumerable<UnifiedItem>> ExecuteCoreAsync(string query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_clientId) ||
                string.IsNullOrWhiteSpace(_clientSecret) ||
                _clientId.StartsWith("replace-with-", StringComparison.OrdinalIgnoreCase) ||
                _clientSecret.StartsWith("replace-with-", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Spotify credentials are missing. Set Apis:Spotify:ClientId and Apis:Spotify:ClientSecret.");
            }

            var accessToken = await GetAccessTokenAsync(cancellationToken);

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"search?q={Uri.EscapeDataString(query)}&type=track&limit=10");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<SpotifySearchResponse>(cancellationToken: cancellationToken)
                ?? new SpotifySearchResponse();

            return payload.Tracks?.Items?.Select(track => new UnifiedItem
            {
                Source = ProviderName,
                Title = track.Name ?? "Unknown track",
                Description = $"Artists: {string.Join(", ", track.Artists?.Select(artist => artist.Name).Where(name => !string.IsNullOrWhiteSpace(name)) ?? Enumerable.Empty<string>())}",
                Category = track.Album?.Name ?? "Track",
                Url = track.ExternalUrls?.Spotify ?? string.Empty,
                RelevanceScore = track.Popularity,
                Date = ParseSpotifyReleaseDate(track.Album?.ReleaseDate)
            }) ?? Enumerable.Empty<UnifiedItem>();
        }

        private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_accessToken) && _expiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return _accessToken;
            }

            await _tokenLock.WaitAsync(cancellationToken);

            try
            {
                if (!string.IsNullOrWhiteSpace(_accessToken) && _expiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(1))
                {
                    return _accessToken;
                }

                using var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                request.Content = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["grant_type"] = "client_credentials"
                    });

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var payload = await response.Content.ReadFromJsonAsync<SpotifyTokenResponse>(cancellationToken: cancellationToken)
                    ?? throw new InvalidOperationException("Spotify token response was empty.");

                _accessToken = payload.AccessToken;
                _expiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(payload.ExpiresIn);

                return _accessToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private static DateTime ParseSpotifyReleaseDate(string? releaseDate)
        {
            if (string.IsNullOrWhiteSpace(releaseDate))
            {
                return DateTime.UtcNow;
            }

            if (DateTime.TryParse(releaseDate, out var parsed))
            {
                return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }

            if (releaseDate.Length == 4 && int.TryParse(releaseDate, out var year))
            {
                return new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }

            return DateTime.UtcNow;
        }
    }
}
