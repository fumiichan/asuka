using System.Text.Json.Serialization;

namespace asuka.Sdk.Providers.Models;

public record GallerySeriesChaptersModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }
}
