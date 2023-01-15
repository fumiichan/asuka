using Newtonsoft.Json;

namespace asuka.Core.Api.Responses;

public record GalleryImageResponse
{
    [JsonProperty("t")]
    public string Type { get; init; }

    [JsonProperty("h")]
    public int Height { get; init; }

    [JsonProperty("w")]
    public int Width { get; init; }
}
