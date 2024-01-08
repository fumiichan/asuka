using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace asuka.Api.Responses;

#nullable disable
public record GalleryImageObjectResponse
{
    [JsonPropertyName("pages")]
    public IReadOnlyList<GalleryImageResponse> Images { get; set; } 
}
