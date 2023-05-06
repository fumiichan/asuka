using System.Text.Json.Serialization;

namespace asuka.Core.Models;

public record GallerySeriesModel
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("artist")]
    public string Artist { get; set; }
    
    [JsonPropertyName("genres")]
    public IReadOnlyList<string> Genres { get; set; }
    
    [JsonPropertyName("chapters")]
    public IReadOnlyList<GallerySeriesChaptersModel> Chapters { get; set; }
}
