using System.Text.Json.Serialization;

namespace asuka.Provider.Nhentai.Contracts;

internal sealed class GalleryConfigResponse
{
    [JsonPropertyName("image_servers")]
    public IEnumerable<string> ImageServers { get; init; } = [];
    
    [JsonPropertyName("thumb_servers")]
    public IEnumerable<string> ThumbServers { get; init; } = [];
}
