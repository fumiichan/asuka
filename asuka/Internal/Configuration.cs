using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using asuka.Model;

namespace asuka.Internal
{
  class Configuration
  {
    private readonly ConfigurationModel ConfigData = new ConfigurationModel();

    public Configuration()
    {
      string configPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.json");
      if (File.Exists(configPath))
      {
        using StreamReader read = new StreamReader(configPath);
        string json = read.ReadToEnd();

        try
        {
          ConfigData = JsonConvert.DeserializeObject<ConfigurationModel>(json);
        } catch (Exception e)
        {
          Console.WriteLine("Failed to read configuration: {0}", e.Message);
          Console.WriteLine("Will use the default values.");
        }
      }
    }

    public string GetConfigurationValue (string key)
    {
      // Todo: Figure out how to find the specific key from the model.
      return key switch
      {
        "preferJapanese" => ConfigData.PreferJapanese.ToString(),
        "parallelTasks" => ConfigData.ConcurrentTasks.ToString(),
        "parallelImageDownload" => ConfigData.ConcurrentImageTasks.ToString(),
        _ => throw new KeyNotFoundException("The configuration key you are looking for cannot be found."),
      };
    }
  }
}
