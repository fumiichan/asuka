using System.Text.Json.Serialization;

namespace asuka.Providers.Nhentai.Api.Responses;

public record GalleryImageObjectResponse
{
    [JsonPropertyName("pages")]
    public IReadOnlyList<GalleryImageResponse> Images { get; set; }
}
