using Newtonsoft.Json;

namespace asuka.Model
{
  public class ConfigurationModel
  {
    [JsonProperty("parallelTasks")]
    public uint ConcurrentTasks { get; set; } = 2;

    [JsonProperty("parallelImageDownload")]
    public uint ConcurrentImageTasks { get; set; } = 2;

    [JsonProperty("preferJapanese")]
    public bool PreferJapanese { get; set; } = false;
  }
}
