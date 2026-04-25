using System.Text.Json.Serialization;

namespace Infrastructure.ExternalApis.GithubApi.Models
{
    public class GithubSearchResponse
    {
        [JsonPropertyName("items")]
        public List<GithubRepository>? Items { get; set; }
    }

    public class GithubRepository
    {
        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("stargazers_count")]
        public int Stars { get; set; }
    }
}
