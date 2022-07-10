using Newtonsoft.Json;

namespace asuka.Configuration;

public class ConfigurationData
{
    [JsonProperty("cookies")]
    public CookieDump[] Cookies { get; set; }
    
    [JsonProperty("user_agent")]
    public string UserAgent { get; set; }

    [JsonProperty("use_tachiyomi_layout")]
    public bool UseTachiyomiLayout { get; set; }
    
    [JsonProperty("console_theme")]
    public string ConsoleTheme { get; set; }
}
