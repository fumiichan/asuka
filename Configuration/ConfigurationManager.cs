﻿using System;
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

    public async Task SetCookies(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Cookie file specified cannot be found.");
        }

        var cookies = await File.ReadAllTextAsync(path, Encoding.UTF8);
        var cookieData = JsonSerializer.Deserialize<CookieDump[]>(cookies);

        if (cookieData == null) return;

        Configuration.Cookies = cookieData;
    }

    public void SetUserAgent(string userAgent)
    {
        Configuration.UserAgent = userAgent;
    }

    public void ToggleTachiyomiLayout(bool value)
    {
        Configuration.UseTachiyomiLayout = value;
    }

    public void ChangeColourTheme(string value)
    {
        Configuration.ConsoleTheme = value;
    }

    public void Reset()
    {
        Configuration = new ConfigurationData();
    }

    public ConfigurationData Values => Configuration;

    public async Task Flush()
    {
        var configPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".asuka/config.json");

        var serializerOptions = new JsonSerializerOptions { WriteIndented = true };
        var data = JsonSerializer.Serialize(Configuration, serializerOptions);
        await File.WriteAllTextAsync(configPath, data);
    }
}
