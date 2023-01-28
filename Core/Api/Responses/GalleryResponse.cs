using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace asuka.Core.Api.Responses;

public record GalleryResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("media_id")]
    public string MediaId { get; set; }

    [JsonPropertyName("title")]
    public GalleryTitleResponse Title { get; set; }

    [JsonPropertyName("images")]
    public GalleryImageObjectResponse Images { get; set; }

    [JsonPropertyName("tags")]
    public IReadOnlyList<GalleryTagResponse> Tags { get; set; }

    [JsonPropertyName("num_pages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("num_favorites")]
    public int TotalFavorites { get; set; }
}
