using System.Reflection;
using System.Text.Json;

namespace asuka.Providers.Nhentai.Configuration;

public static class OverrideConfigurations
{
    public static OverrideConfigurationData GetConfiguration()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var configPath = Path.Combine(assemblyDir, "provider.nhentai-config.json");
        
        if (!File.Exists(configPath))
        {
            return new OverrideConfigurationData();
        }
        
        try
        {
            var file = File.ReadAllText(configPath);
            var cookieData = JsonSerializer.Deserialize<OverrideConfigurationData>(file);

            return cookieData is null
                ? new OverrideConfigurationData()
                : cookieData;
        }
        catch
        {
            // ignored
        }

        return new OverrideConfigurationData();
    }
}
