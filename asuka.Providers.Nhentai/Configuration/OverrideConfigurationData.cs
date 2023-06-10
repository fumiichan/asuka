using System.Text.Json.Serialization;

namespace asuka.Providers.Nhentai.Configuration;

public class OverrideConfigurationData
{
    [JsonPropertyName("userAgent")]
    public string UserAgent { get; init; } =
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36";
    
    [JsonPropertyName("apiHostname")]
    public string ApiHostname { get; init; } = "https://nhentai.net";
    
    [JsonPropertyName("imageHostname")]
    public string ImageHostname { get; init; } = "https://i.nhentai.net";
}
