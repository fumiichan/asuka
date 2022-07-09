using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace asuka.Configuration;

public class ConfigurationManager : IConfigurationManager
{
    protected readonly ConfigurationData Configuration;

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
        Configuration = JsonConvert.DeserializeObject<ConfigurationData>(data);
    }

    public async Task SetCookiesAsync(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Cookie file specified cannot be found.");
        }

        var cookies = await File.ReadAllTextAsync(path, Encoding.UTF8);
        var cookieData = JsonConvert.DeserializeObject<CookieDump[]>(cookies);

        if (cookieData == null) return;

        foreach (var cookie in cookieData)
        {
            switch (cookie.Name)
            {
                case "cf_clearance":
                    Configuration.CloudflareClearance = cookie.Value;
                    break;
                case "csrftoken":
                    Configuration.CsrfToken = cookie.Value;
                    break;
            }
        }
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

    public ConfigurationData Values => Configuration;

    private async Task FlushAsync()
    {
        var configPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".asuka/config.json");
        
        var data = JsonConvert.SerializeObject(Configuration);
        await File.WriteAllTextAsync(configPath, data);
    }
}
