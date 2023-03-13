using System.Text.Json.Serialization;

namespace asuka.Api.Responses;

public record GalleryImageResponse
{
    [JsonPropertyName("t")]
    public string Type { get; set; }

    [JsonPropertyName("h")]
    public int Height { get; set; }

    [JsonPropertyName("w")]
    public int Width { get; set; }
}
