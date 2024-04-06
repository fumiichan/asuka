using System.Text.Json.Serialization;

namespace asuka.Provider.Nhentai;

internal sealed class JsonCookie
{
    [JsonPropertyName("domain")]
    public string Domain { get; init; }
    
    [JsonPropertyName("hostOnly")]
    public bool HostOnly { get; init; }
    
    [JsonPropertyName("httpOnly")]
    public bool HttpOnly { get; init; }
    
    [JsonPropertyName("name")]
    public string Name { get; init; }
    
    [JsonPropertyName("path")]
    public string Path { get; init; }
    
    [JsonPropertyName("sameSite")]
    public string SameSite { get; init; }
    
    [JsonPropertyName("secure")]
    public bool Secure { get; init; }
    
    [JsonPropertyName("value")]
    public string Value { get; init; }
}
