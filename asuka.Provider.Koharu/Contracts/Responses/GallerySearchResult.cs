using System.Text.Json.Serialization;

namespace asuka.Provider.Koharu.Contracts.Responses;

internal sealed class GallerySearchResult
{
    [JsonPropertyName("entries")]
    public List<GalleryInfoResponse> Entries { get; set; } = [];
    
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
    
    [JsonPropertyName("page")]
    public int Page { get; set; }
    
    [JsonPropertyName("total")]
    public int Total { get; set; }
}