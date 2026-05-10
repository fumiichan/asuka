#pragma warning disable CS8618 
// ReSharper disable ClassNeverInstantiated.Global

using System.Text.Json.Serialization;

namespace asuka.Provider.Nhentai.Contracts;

internal sealed class GalleryResponse
{
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; init; }
    
    [JsonPropertyName("media_id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int MediaId { get; init; }
    
    [JsonPropertyName("title")]
    public Titles Title { get; init; }
    
    [JsonPropertyName("cover")]
    public CoverObject Cover { get; init; }
    
    [JsonPropertyName("thumbnail")]
    public CoverObject Thumbnail { get; init; }
    
    [JsonPropertyName("scanlator")]
    public string? Scanlator { get; init; }
    
    [JsonPropertyName("upload_date")]
    public long UploadDate { get; init; }
    
    [JsonPropertyName("tags")]
    public IEnumerable<Tag> Tags { get; init; }
    
    [JsonPropertyName("num_pages")]
    public int NumPages { get; init; }
    
    [JsonPropertyName("num_favorites")]
    public int NumFavorites { get; init; }
    
    [JsonPropertyName("pages")]
    public IEnumerable<Page> Pages { get; init; }
    
    internal sealed class Titles
    {
        [JsonPropertyName("japanese")]
        public string Japanese { get; init; }
        
        [JsonPropertyName("english")]
        public string English { get; init; }
        
        [JsonPropertyName("pretty")]
        public string Pretty { get; init; }
    }

    internal sealed class Page
    {
        [JsonPropertyName("number")]
        public int Number { get; init; }
        
        [JsonPropertyName("path")]
        public string Path { get; init; }
        
        [JsonPropertyName("width")]
        public int Width { get; init; }
        
        [JsonPropertyName("height")]
        public int Height { get; init; }
        
        [JsonPropertyName("thumbnail")]
        public string Thumbnail { get; init; }
        
        [JsonPropertyName("thumbnail_width")]
        public int ThumbnailWidth { get; init; }
        
        [JsonPropertyName("thumbnail_height")]
        public int ThumbnailHeight { get; init; }
    }

    internal sealed class Tag
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }
        
        [JsonPropertyName("type")]
        public string Type { get; init; }
        
        [JsonPropertyName("name")]
        public string Name { get; init; }
        
        [JsonPropertyName("slug")]
        public string Slug { get; init; }
        
        [JsonPropertyName("url")]
        public string Url { get; init; }
        
        [JsonPropertyName("count")]
        public int Count { get; init; }
    }

    internal sealed class CoverObject
    {
        [JsonPropertyName("path")]
        public string Path { get; init; } = string.Empty;
        
        [JsonPropertyName("width")]
        public int Width { get; init; }
        
        [JsonPropertyName("height")]
        public int Height { get; init; }
    }
}
