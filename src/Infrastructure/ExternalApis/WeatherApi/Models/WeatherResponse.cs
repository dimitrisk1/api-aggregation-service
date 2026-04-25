namespace Core_Infrastructure.ExternalApis.WeatherApi.Models
{
    public class WeatherApiResponse
    {
        public string? LocationName { get; set; }
        public double Temperature { get; set; }
        public string? Condition { get; set; }
    }
}
