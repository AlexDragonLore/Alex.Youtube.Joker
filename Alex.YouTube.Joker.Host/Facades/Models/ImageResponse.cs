using System.Text.Json.Serialization;

namespace Alex.YouTube.Joker.Host.Facades.Models;

public class ImageResponse
{
    [JsonPropertyName("data")]
    public List<ImageData>? Data { get; set; }
}

public class ImageData
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("b64_json")]
    public string? B64Json { get; set; }
}