

using System.Text.Json.Serialization;

namespace Core_Infrastructure.ExternalApis.NewsApi.Models
{
    public class NewsApiResponse
    {
        public List<NewsArticle>? Articles { get; set; }
    }

    public class NewsArticle
    {
        public string? Headline { get; set; }
        public string? Summary { get; set; }
        public DateTime PublishedAt { get; set; }
    }

}
