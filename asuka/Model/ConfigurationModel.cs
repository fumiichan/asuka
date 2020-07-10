using Newtonsoft.Json;

namespace asuka.Model
{
  public class ConfigurationModel
  {
    private uint _concurrentImageTasks = 2;

    [JsonProperty("parallelImageDownload")]
    public uint ConcurrentImageTasks
    {
      get
      {
        return _concurrentImageTasks;
      }
      set
      {
        if (value >= 1)
        {
          _concurrentImageTasks = value;
        }
      }
    }

    [JsonProperty("preferJapanese")]
    public bool PreferJapanese { get; set; } = false;
  }
}
