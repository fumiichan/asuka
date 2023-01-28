using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace asuka.Core.Api.Responses;

public record GalleryListResponse
{
    [JsonPropertyName("result")]
    public IReadOnlyList<GalleryResponse> Result { get; set; }
}
