using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace asuka.Configuration;

public class ConfigurationManager : IConfigurationManager
{
    protected ConfigurationData Configuration;

    public ConfigurationManager()
    {
        var configRoot = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".asuka");
        var configPath = Path.Join(configRoot, "config.json");

        if (!File.Exists(configPath))
        {
            Directory.CreateDirectory(configRoot);
            Configuration = new ConfigurationData();
            return;
        }

        var data = File.ReadAllText(configPath, Encoding.UTF8);
        Configuration = JsonSerializer.Deserialize<ConfigurationData>(data);
    }

    public async Task SetCookiesAsync(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Cookie file specified cannot be found.");
        }

        var cookies = await File.ReadAllTextAsync(path, Encoding.UTF8);
        var cookieData = JsonSerializer.Deserialize<CookieDump[]>(cookies);

        if (cookieData == null) return;

        Configuration.Cookies = cookieData;
        await FlushAsync();
    }

    public async Task SetUserAgentAsync(string userAgent)
    {
        Configuration.UserAgent = userAgent;
        await FlushAsync();
    }

    public async Task ToggleTachiyomiLayoutAsync(bool value)
    {
        Configuration.UseTachiyomiLayout = value;
        await FlushAsync();
    }

    public async Task ChangeColourThemeAsync(string value)
    {
        Configuration.ConsoleTheme = value;
        await FlushAsync();
    }

    public async Task ResetAsync()
    {
        Configuration = new ConfigurationData();
        await FlushAsync();
    }

    public ConfigurationData Values => Configuration;

    private async Task FlushAsync()
    {
        var configPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".asuka/config.json");

        var serializerOptions = new JsonSerializerOptions { WriteIndented = true };
        var data = JsonSerializer.Serialize(Configuration, serializerOptions);
        await File.WriteAllTextAsync(configPath, data);
    }
}
