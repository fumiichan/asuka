using System.Text.Json.Serialization;

namespace asuka.Provider.Hitomi.Contracts.Responses;

internal sealed class FileEntry
{
    [JsonPropertyName("hasjxl")]
    public int HasJxl { get; set; }
    
    [JsonPropertyName("haswebp")]
    public int HasWebp { get; set; }
    
    [JsonPropertyName("hasavif")]
    public int HasAvif { get; set; }
    
    [JsonPropertyName("width")]
    public int Width { get; set; }
    
    [JsonPropertyName("height")]
    public int Height { get; set; }
    
    [JsonPropertyName("hash")]
    public required string Hash { get; set; }
    
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
