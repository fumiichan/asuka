using Newtonsoft.Json;

namespace asuka.Api.Responses;

public record GallerySearchResponse : GalleryListResponse
{
    [JsonProperty("num_pages")]
    public int TotalPages { get; init; }

    [JsonProperty("per_page")]
    public int TotalItemsPerPage { get; init; }
}
