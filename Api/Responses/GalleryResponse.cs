using System.Collections.Generic;
using Newtonsoft.Json;

namespace asuka.Api.Responses
{
    public record GalleryResponse
    {
        [JsonProperty("id")]
        public int Id { get; init; }
        
        [JsonProperty("media_id")]
        public int MediaId { get; init; }
        
        [JsonProperty("title")]
        public GalleryTitleResponse Title { get; init; }
        
        [JsonProperty("images")]
        public GalleryImageObjectResponse Images { get; init; }
        
        [JsonProperty("tags")]
        public IReadOnlyList<GalleryTagResponse> Tags { get; init; }
        
        [JsonProperty("num_pages")]
        public int TotalPages { get; init; }
        
        [JsonProperty("num_favorites")]
        public int TotalFavorites { get; init; }
    }
}