using System.Text.Json.Serialization;

namespace asuka.Api.Responses;

#nullable disable
public record GalleryTagResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
