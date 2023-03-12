using System.Text.Json.Serialization;

namespace asuka.Core.Models;

public record GallerySeriesChaptersModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }
}
