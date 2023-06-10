using System.Text.Json.Serialization;

namespace asuka.Providers.Nhentai.Api.Responses;

public record GalleryListResponse
{
    [JsonPropertyName("result")]
    public IReadOnlyList<GalleryResponse> Result { get; set; }
}
