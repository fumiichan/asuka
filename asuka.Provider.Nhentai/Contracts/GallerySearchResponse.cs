using System.Text.Json.Serialization;

namespace asuka.Provider.Nhentai.Contracts;

internal sealed class GallerySearchResponse
{
    [JsonPropertyName("result")]
    public IEnumerable<GalleryResponse> Result { get; init; }
    
    [JsonPropertyName("num_pages")]
    public int TotalPages { get; init; }
    
    [JsonPropertyName("per_page")]
    public int TotalItemsPerPage { get; init; }
}
