using System.Text.Json.Serialization;

namespace asuka.Api.Responses;

public record GalleryListResponse
{
    [JsonPropertyName("result")]
    public IReadOnlyList<GalleryResponse> Result { get; set; }
}
