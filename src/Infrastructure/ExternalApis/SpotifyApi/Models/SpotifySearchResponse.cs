using System.Text.Json.Serialization;

namespace Infrastructure.ExternalApis.SpotifyApi.Models
{
    public class SpotifySearchResponse
    {
        [JsonPropertyName("tracks")]
        public SpotifyTrackContainer? Tracks { get; set; }
    }

    public class SpotifyTrackContainer
    {
        [JsonPropertyName("items")]
        public List<SpotifyTrack>? Items { get; set; }
    }

    public class SpotifyTrack
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("artists")]
        public List<SpotifyArtist>? Artists { get; set; }

        [JsonPropertyName("album")]
        public SpotifyAlbum? Album { get; set; }

        [JsonPropertyName("external_urls")]
        public SpotifyExternalUrls? ExternalUrls { get; set; }

        [JsonPropertyName("popularity")]
        public int Popularity { get; set; }
    }

    public class SpotifyArtist
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class SpotifyAlbum
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }
    }

    public class SpotifyExternalUrls
    {
        [JsonPropertyName("spotify")]
        public string? Spotify { get; set; }
    }

    public class SpotifyTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
