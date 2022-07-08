using Newtonsoft.Json;

namespace asuka.Configuration;

public class ConfigurationData
{
    [JsonProperty("cloudflare_clearance")]
    public string CloudflareClearance { get; set; }
    
    [JsonProperty("csrf_token")]
    public string CsrfToken { get; set; }
    
    [JsonProperty("user_agent")]
    public string UserAgent { get; set; }
}
