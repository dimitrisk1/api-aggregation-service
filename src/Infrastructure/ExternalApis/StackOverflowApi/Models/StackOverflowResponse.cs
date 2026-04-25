using System.Text.Json.Serialization;

namespace Infrastructure.ExternalApis.StackOverflowApi.Models
{
    public class StackOverflowResponse
    {
        [JsonPropertyName("items")]
        public List<StackOverflowQuestion>? Items { get; set; }
    }

    public class StackOverflowQuestion
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("is_answered")]
        public bool IsAnswered { get; set; }

        [JsonPropertyName("last_activity_date")]
        public long LastActivityDate { get; set; }
    }
}
