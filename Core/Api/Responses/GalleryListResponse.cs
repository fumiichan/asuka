using System.Collections.Generic;
using Newtonsoft.Json;

namespace asuka.Api.Responses;

public record GalleryListResponse
{
    [JsonProperty("result")]
    public IReadOnlyList<GalleryResponse> Result { get; init; }
}
