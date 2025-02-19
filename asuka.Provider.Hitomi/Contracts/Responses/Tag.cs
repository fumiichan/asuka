using System.Text.Json.Serialization;

namespace asuka.Provider.Hitomi.Contracts.Responses;

internal sealed class Tag
{
    [JsonPropertyName("tag")]
    public required string Name { get; init; }
    
    [JsonPropertyName("url")]
    public required string Url { get; init; }
}
