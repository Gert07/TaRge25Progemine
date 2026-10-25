using System.Text.Json.Serialization;

namespace TaRge25Shop.Core.Dto
{
    public class OpenWeatherRootDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("weather")]
        public List<OpenWeatherConditionDto>? Weather { get; set; }

        [JsonPropertyName("main")]
        public OpenWeatherMainDto? Main { get; set; }

        [JsonPropertyName("wind")]
        public OpenWeatherWindDto? Wind { get; set; }

        [JsonPropertyName("sys")]
        public OpenWeatherSystemDto? Sys { get; set; }

        [JsonPropertyName("clouds")]
        public OpenWeatherCloudsDto? Clouds { get; set; }
    }

    public class OpenWeatherConditionDto
    {
        [JsonPropertyName("main")]
        public string? Main { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("icon")]
        public string? Icon { get; set; }
    }

    public class OpenWeatherMainDto
    {
        [JsonPropertyName("temp")]
        public double? Temperature { get; set; }

        [JsonPropertyName("feels_like")]
        public double? FeelsLike { get; set; }

        [JsonPropertyName("humidity")]
        public int? Humidity { get; set; }

        [JsonPropertyName("pressure")]
        public int? Pressure { get; set; }
    }

    public class OpenWeatherWindDto
    {
        [JsonPropertyName("speed")]
        public double? Speed { get; set; }
    }

    public class OpenWeatherSystemDto
    {
        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }

    public class OpenWeatherCloudsDto
    {
        [JsonPropertyName("all")]
        public int? All { get; set; }
    }
}
