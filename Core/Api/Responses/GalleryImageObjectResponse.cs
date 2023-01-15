using System.Collections.Generic;
using Newtonsoft.Json;

namespace asuka.Core.Api.Responses;

public record GalleryImageObjectResponse
{
    [JsonProperty("pages")]
    public IReadOnlyList<GalleryImageResponse> Images { get; init; }
}
