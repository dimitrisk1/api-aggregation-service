namespace Infrastructure.ExternalApis.TwitterApi.Models
{
    public class TweetResponse
    {
        public IList<TweetData> Data { get; set; } = new List<TweetData>();
    }

    public class TweetData
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }
}
