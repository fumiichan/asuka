using Newtonsoft.Json;

namespace asuka.Configuration;

public record CookieDump
{
    [JsonProperty("domain")]
    public string Domain { get; init; }
    
    [JsonProperty("httpOnly")]
    public bool HttpOnly { get; init; }
    
    [JsonProperty("secure")]
    public bool Secure { get; init; }
    
    [JsonProperty("name")]
    public string Name { get; init; }
    
    [JsonProperty("value")]
    public string Value { get; init; }
}
