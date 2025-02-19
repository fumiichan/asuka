// ReSharper disable ClassNeverInstantiated.Global

using System.Text.Json.Serialization;

namespace asuka.Provider.Koharu.Contracts.Responses;

internal sealed class GalleryInfoResponse
{
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; init; }
    
    [JsonPropertyName("data")]
    public Dictionary<string, GalleryImageDetails> Data { get; init; } = new();
    
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    [JsonPropertyName("public_key")]
    public string PublicKey { get; init; } = string.Empty;
    
    [JsonPropertyName("rels")]
    public List<RelevantGallery> RelevantGalleries { get; init; } = [];
    
    [JsonPropertyName("tags")]
    public List<TagDetails> Tags { get; init; } = [];

    [JsonPropertyName("title")]
    public string Title { get; init; } = "Unknown Title";
    
    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; init; }

    internal sealed class GalleryImageDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }
        
        [JsonPropertyName("public_key")]
        public string PublicKey { get; init; } = string.Empty;
        
        [JsonPropertyName("size")]
        public long Size { get; init; }
    }

    internal sealed class RelevantGallery
    {
        [JsonPropertyName("created_at")]
        public long CreatedAt { get; init; }
        
        [JsonPropertyName("id")]
        public int Id { get; init; }
        
        [JsonPropertyName("language")]
        public string Language { get; init; } = string.Empty;
        
        [JsonPropertyName("pages")]
        public int Pages { get; init; }
        
        [JsonPropertyName("public_key")]
        public string PublicKey { get; init; } = string.Empty;
        
        [JsonPropertyName("tags")]
        public List<TagDetails> Tags { get; init; } = [];
    }

    internal sealed class TagDetails
    {
        [JsonPropertyName("namespace")]
        public GalleryTag Namespace { get; init; } = GalleryTag.NoNamespace;
        
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;
    }
}
