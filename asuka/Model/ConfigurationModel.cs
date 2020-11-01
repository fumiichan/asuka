using System.ComponentModel.DataAnnotations;

namespace asuka.Model
{
  public class ConfigurationModel
  {
    private uint _concurrentTasks = 2;
    private uint _concurrentImageTasks = 2;

    [Required]
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

    [Required]
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

    [Required]
    public bool PreferJapanese { get; set; } = false;
  }
}
