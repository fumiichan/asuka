using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using asuka.Model;

namespace asuka.Internal
{
  class Configuration
  {
    public readonly ConfigurationModel ConfigData = new ConfigurationModel();

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
  }
}
