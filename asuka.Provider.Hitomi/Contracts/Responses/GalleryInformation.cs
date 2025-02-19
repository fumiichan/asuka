using System.Text.Json.Serialization;

namespace asuka.Provider.Hitomi.Contracts.Responses;

internal sealed class GalleryInformation
{
    [JsonPropertyName("characters")]
    public IEnumerable<Character>? Characters { get; init; } = [];
    
    [JsonPropertyName("tags")]
    public IEnumerable<Tag>? Tags { get; init; } = [];
    
    [JsonPropertyName("type")]
    public string? Type { get; init; }
    
    [JsonPropertyName("files")]
    public IEnumerable<FileEntry> Files { get; init; } = [];
    
    [JsonPropertyName("groups")]
    public IEnumerable<Group>? Groups { get; init; } = [];

    [JsonPropertyName("parodys")]
    public IEnumerable<Parody>? Parodies { get; init; } = [];

    [JsonPropertyName("related")]
    public IEnumerable<int>? Related { get; init; } = [];
    
    [JsonPropertyName("date")]
    public required string Date { get; init; }
    
    [JsonPropertyName("title")]
    public required string Title { get; init; }
    
    [JsonPropertyName("japanese_title")]
    public string? JapaneseTitle { get; init; }
}
