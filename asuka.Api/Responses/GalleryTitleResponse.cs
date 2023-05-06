using System.Text.Json.Serialization;

namespace asuka.Api.Responses;

public record GalleryTitleResponse
{
    [JsonPropertyName("japanese")]
    public string Japanese { get; set; }

    [JsonPropertyName("english")]
    public string English { get; set; }

    [JsonPropertyName("pretty")]
    public string Pretty { get; set; }
}
