using System.Text.Json.Serialization;

namespace asuka.Provider.Nhentai.Contracts;

internal sealed class GalleryListResponse
{
    [JsonPropertyName("result")]
    public IEnumerable<GalleryResponse> Result { get; init; }
}