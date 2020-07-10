using Newtonsoft.Json;

namespace asuka.Model
{
  public class ConfigurationModel
  {
    private uint _concurrentTasks = 2;
    private uint _concurrentImageTasks = 2;

    [JsonProperty("parallelTasks")]
    public uint ConcurrentTasks
    {
      get
      {
        return _concurrentTasks;
      }
      set
      {
        if (value >= 1)
        {
          _concurrentTasks = value;
        }
      }
    }

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
