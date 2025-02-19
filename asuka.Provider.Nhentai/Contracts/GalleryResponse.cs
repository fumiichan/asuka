#pragma warning disable CS8618 
// ReSharper disable ClassNeverInstantiated.Global

using System.Text.Json.Serialization;

namespace asuka.Provider.Nhentai.Contracts;

internal sealed class GalleryResponse
{
    internal sealed class Titles
    {
        [JsonPropertyName("japanese")]
        public string Japanese { get; init; }
        
        [JsonPropertyName("english")]
        public string English { get; init; }
        
        [JsonPropertyName("pretty")]
        public string Pretty { get; init; }
    }

    internal sealed class GalleryImages
    {
        [JsonPropertyName("pages")]
        public IEnumerable<Page> Pages { get; init; }
    }

    internal sealed class Page
    {
        [JsonPropertyName("t")]
        public string Format { get; init; }
        
        [JsonPropertyName("h")]
        public int Height { get; init; }
        
        [JsonPropertyName("w")]
        public int Width { get; init; }
    }

    internal sealed class Tag
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }
        
        [JsonPropertyName("type")]
        public string Type { get; init; }
        
        [JsonPropertyName("name")]
        public string Name { get; init; }
    }

    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; init; }
    
    [JsonPropertyName("media_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int MediaId { get; init; }
    
    [JsonPropertyName("title")]
    public Titles Title { get; init; }
    
    [JsonPropertyName("images")]
    public GalleryImages Images  { get; init; }
    
    [JsonPropertyName("tags")]
    public IEnumerable<Tag> Tags { get; init; }
    
    [JsonPropertyName("num_pages")]
    public int TotalPages { get; init; }
}
