

using System.Text.Json.Serialization;

namespace Core_Infrastructure.ExternalApis.NewsApi.Models
{
    public class NewsApiResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }

        [JsonPropertyName("articles")]
        public List<Article> Articles { get; set; } = new();
    }
}
