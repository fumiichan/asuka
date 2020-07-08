using Newtonsoft.Json;

namespace asuka.Model
{
  public class ConfigurationModel
  {
    [JsonProperty("parallelTasks")]
    public int ConcurrentTasks { get; set; } = 2;

    [JsonProperty("parallelImageDownload")]
    public int ConcurrentImageTasks { get; set; } = 2;

    [JsonProperty("preferJapanese")]
    public bool PreferJapanese { get; set; } = false;
  }
}
