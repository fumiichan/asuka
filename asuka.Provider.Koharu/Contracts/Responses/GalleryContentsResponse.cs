using System.Text.Json.Serialization;

namespace asuka.Provider.Koharu.Contracts.Responses;

internal sealed class GalleryContentsResponse
{
    [JsonPropertyName("base")]
    public string Base { get; set; } = string.Empty;

    [JsonPropertyName("entries")]
    public List<Entry> Entries { get; set; } = [];

    internal sealed class Entry
    {
        [JsonPropertyName("dimensions")]
        public int[] Dimensions { get; init; } = [];
        
        [JsonPropertyName("path")]
        public string Path { get; init; } = string.Empty;
    }
}
