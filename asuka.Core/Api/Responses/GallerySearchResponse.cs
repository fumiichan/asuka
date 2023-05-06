using System.Text.Json.Serialization;

namespace asuka.Core.Api.Responses;

public record GallerySearchResponse : GalleryListResponse
{
    [JsonPropertyName("num_pages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("per_page")]
    public int TotalItemsPerPage { get; set; }
}
