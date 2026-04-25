using System.Text.Json.Serialization;

namespace Core_Infrastructure.ExternalApis.NewsApi.Models
{
    public class Article
    {
        [JsonPropertyName("source")]
        public Source Source { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("urlToImage")]
        public string? UrlToImage { get; set; }

        [JsonPropertyName("publishedAt")]
        public DateTime PublishedAt { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    public class Source
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}