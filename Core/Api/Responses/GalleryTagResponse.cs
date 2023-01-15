using Newtonsoft.Json;

namespace asuka.Core.Api.Responses;

public record GalleryTagResponse
{
    [JsonProperty("id")]
    public int Id { get; init; }

    [JsonProperty("type")]
    public string Type { get; init; }

    [JsonProperty("name")]
    public string Name { get; init; }

    [JsonProperty("url")]
    public string Url { get; init; }

    [JsonProperty("count")]
    public int Count { get; init; }
}
