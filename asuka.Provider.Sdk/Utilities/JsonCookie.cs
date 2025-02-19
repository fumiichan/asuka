// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Text.Json.Serialization;

namespace asuka.Provider.Sdk.Utilities;

internal sealed class JsonCookie
{
    [JsonPropertyName("domain")]
    public string Domain { get; init; } = string.Empty;
    
    [JsonPropertyName("hostOnly")]
    public bool HostOnly { get; init; }
    
    [JsonPropertyName("httpOnly")]
    public bool HttpOnly { get; init; }
    
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
    
    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;
    
    [JsonPropertyName("sameSite")]
    public string SameSite { get; init; } = string.Empty;
    
    [JsonPropertyName("secure")]
    public bool Secure { get; init; }
    
    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;
}
