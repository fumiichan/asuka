using System;
using System.IO;
using YamlDotNet.Serialization;
using asuka.Model;

namespace asuka.Internal
{
  public class Configuration
  {
    public readonly ConfigurationModel ConfigData = new ConfigurationModel();

    public Configuration()
    {
      string yamlConfig = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.yaml");
      if (File.Exists(yamlConfig))
      {
        using var reader = File.OpenText(yamlConfig);
        var deserializer = new Deserializer();

        try
        {
          ConfigData = deserializer.Deserialize<ConfigurationModel>(reader);
        }
        catch (Exception e)
        {
          Console.WriteLine(
            "Your Configuration has Errors in it. Delete the configuration to regenerate it again.\n" +
            "Currently any changes on your config is ignored and defaults will be used."
          );
          Console.WriteLine(e.Message);
        }
      }
      else
      {
        // Try to save a yaml file.
        var serialiser = new Serializer();
        string serialised = serialiser.Serialize(ConfigData);

        File.WriteAllText(yamlConfig, serialised);
      }
    }
  }
}
