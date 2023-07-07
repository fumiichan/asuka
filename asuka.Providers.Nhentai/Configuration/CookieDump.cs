using System.Text.Json.Serialization;

namespace asuka.Providers.Nhentai.Configuration;

public record CookieDump
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; }
    
    [JsonPropertyName("httpOnly")]
    public bool HttpOnly { get; set; }
    
    [JsonPropertyName("secure")]
    public bool Secure { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("value")]
    public string Value { get; set; }
}
