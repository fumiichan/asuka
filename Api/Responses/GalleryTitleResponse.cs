using Newtonsoft.Json;

namespace asuka.Api.Responses
{
    public record GalleryTitleResponse
    {
        [JsonProperty("japanese")]
        public string Japanese { get; init; }
        
        [JsonProperty("english")]
        public string English { get; init; }
        
        [JsonProperty("pretty")]
        public string Pretty { get; init; }
    }
}