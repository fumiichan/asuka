using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace asuka.Api.Responses;

#nullable disable
public record GalleryListResponse
{
    [JsonPropertyName("result")]
    public IReadOnlyList<GalleryResponse> Result { get; set; }
}
