using Newtonsoft.Json;

namespace asuka.Model
{
  public class ConfigurationModel
  {
    [JsonProperty("parallelTasks")]
    public uint ConcurrentTasks {
      get
      {
        return 2;
      }
      set
      {
        if (value >= 1)
        {
          ConcurrentTasks = value;
        }
      }
    }

    [JsonProperty("parallelImageDownload")]
    public uint ConcurrentImageTasks
    {
      get
      {
        return 2;
      }
      set
      {
        if (value >= 1)
        {
          ConcurrentImageTasks = value;
        }
      }
    }

    [JsonProperty("preferJapanese")]
    public bool PreferJapanese { get; set; } = false;
  }
}
