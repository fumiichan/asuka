using System.Text.Json.Serialization;

namespace asuka.Api.Responses;

public record GalleryImageObjectResponse
{
    [JsonPropertyName("pages")]
    public IReadOnlyList<GalleryImageResponse> Images { get; set; }
}
