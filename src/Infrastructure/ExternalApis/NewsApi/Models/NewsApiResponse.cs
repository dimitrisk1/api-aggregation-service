using System.Text.Json.Serialization;

namespace Infrastructure.ExternalApis.NewsApi.Models
{
    public class NewsApiResponse
    {
        [JsonPropertyName("articles")]
        public List<NewsApiArticle>? Articles { get; set; }
    }

    public class NewsApiArticle
    {
        [JsonPropertyName("source")]
        public NewsApiSource? Source { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("publishedAt")]
        public DateTime PublishedAt { get; set; }
    }

    public class NewsApiSource
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
