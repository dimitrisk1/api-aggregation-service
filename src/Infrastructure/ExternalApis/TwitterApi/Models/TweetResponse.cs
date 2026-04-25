namespace Infrastructure.ExternalApis.TwitterApi.Models
{
    public class TwitterSearchResponse
    {
        public List<Tweet>? Data { get; set; }
    }


    public class Tweet
    {
        public string? Text { get; set; }
        public string? AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
    }


}
