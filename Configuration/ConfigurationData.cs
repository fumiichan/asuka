using System.Text.Json.Serialization;

namespace asuka.Configuration;

public class ConfigurationData
{
    [JsonPropertyName("cookies")]
    public CookieDump[] Cookies { get; set; }
    
    [JsonPropertyName("user_agent")]
    public string UserAgent { get; set; }

    [JsonPropertyName("use_tachiyomi_layout")]
    public bool UseTachiyomiLayout { get; set; }
    
    [JsonPropertyName("console_theme")]
    public string ConsoleTheme { get; set; }
}
