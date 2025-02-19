using System.Text.Json.Serialization;

namespace asuka.Provider.Hitomi.Contracts.Responses;

internal sealed class Parody
{
    [JsonPropertyName("parody")]
    public required string Name { get; set; }
    
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}
