using System.Text.Json.Serialization;

namespace asuka.Provider.Nhentai.Contracts;

internal sealed class GallerySearchResponse
{
    [JsonPropertyName("result")]
    public IEnumerable<ResultObject> Result { get; init; } = [];
    
    [JsonPropertyName("num_pages")]
    public int NumPages { get; init; }
    
    [JsonPropertyName("per_page")]
    public int PerPage { get; init; }
    
    [JsonPropertyName("total")]
    public int Total { get; init; }

    internal sealed class ResultObject
    {
        [JsonPropertyName("id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int Id { get; init; }
        
        [JsonPropertyName("media_id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int MediaId { get; init; }
        
        [JsonPropertyName("english_title")]
        public string EnglishTitle { get; init; } = string.Empty;
        
        [JsonPropertyName("japanese_title")]
        public string JapaneseTitle { get; init; } = string.Empty;
        
        [JsonPropertyName("thumbnail")]
        public string Thumbnail { get; init; } = string.Empty;
        
        [JsonPropertyName("thumbnail_width")]
        public int ThumbnailWidth { get; init; }
        
        [JsonPropertyName("thumbnail_height")]
        public int ThumbnailHeight { get; init; }
        
        [JsonPropertyName("num_pages")]
        public int NumPages { get; init; }
        
        [JsonPropertyName("tag_ids")]
        public IEnumerable<int> TagIds { get; init; } = [];
        
        [JsonPropertyName("blacklisted")]
        public bool Blacklisted { get; init; }
    }
}
