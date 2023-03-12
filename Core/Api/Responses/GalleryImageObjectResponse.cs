using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace asuka.Core.Api.Responses;

public record GalleryImageObjectResponse
{
    [JsonPropertyName("pages")]
    public IReadOnlyList<GalleryImageResponse> Images { get; set; }
}
