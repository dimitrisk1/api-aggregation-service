using System.Text.Json.Serialization;

namespace Infrastructure.ExternalApis.WeatherApi.Models
{
    public class OpenWeatherResponse
    {
        [JsonPropertyName("id")]
        public long CityId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("dt")]
        public long Timestamp { get; set; }

        [JsonPropertyName("main")]
        public OpenWeatherMain? Main { get; set; }

        [JsonPropertyName("weather")]
        public List<OpenWeatherCondition>? Weather { get; set; }
    }

    public class OpenWeatherMain
    {
        [JsonPropertyName("temp")]
        public double Temperature { get; set; }

        [JsonPropertyName("humidity")]
        public int Humidity { get; set; }
    }

    public class OpenWeatherCondition
    {
        [JsonPropertyName("main")]
        public string? Main { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
