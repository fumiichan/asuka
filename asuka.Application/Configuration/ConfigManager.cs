using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Configuration;

public class ConfigManager : IConfigManager
{
    private Dictionary<string, string> _config;
    private readonly ILogger _logger;

    public ConfigManager(ILogger logger)
    {
        _logger = logger;
        Initialize();
    }

    private void Initialize()
    {
        var configRoot = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".asuka");
        var configPath = Path.Join(configRoot, "config.conf");
        
        _logger.LogInformation("Configuration is located at {ConfigPath}", configPath);

        if (!File.Exists(configPath))
        {
            _logger.LogWarning("Configuration cannot be found on path, Config not exist on: {ConfigPath}", configPath);
            
            Directory.CreateDirectory(configRoot);
            _config = GetDefaults();

            return;
        }

        try
        {
            var data = File.ReadAllText(configPath, Encoding.UTF8);
            _config = ReadConfiguration(data);
            
            _logger.LogInformation("Configuration loaded with values = {@Config}", _config);
        }
        catch (Exception e)
        {
            _logger.LogError("Exception occured at reading configuration: {@Exception}", e);
            _config = GetDefaults();
        }
    }

    private Dictionary<string, string> GetDefaults()
    {
        return new Dictionary<string, string>
        {
            {
                "tui.progress", "progress"
            }
        };
    }

    private Dictionary<string, string> ReadConfiguration(string fileData)
    {
        var config = fileData.Split("\n");

        var dict = new Dictionary<string, string>();
        foreach (var value in config)
        {
            var regex = new Regex("^([a-z1-9.]+)=([a-z1-9])+$");
            if (!regex.IsMatch(value))
            {
                continue;
            }

            var configValue = value.Split('=');
            dict.Add(configValue[0], configValue[1]);
        }

        // Ensure we populate all configuraiton options
        foreach (var value in GetDefaults())
        {
            dict.TryAdd(value.Key, value.Value);
        }

        return dict;
    }

    public void SetValue(string key, string value)
    {
        _config[key] = value;
    }

    public string GetValue(string key)
    {
        return _config.TryGetValue(key, out var data) ? data : null;
    }

    public IEnumerable<(string, string)> GetAllValues()
    {
        return _config.Select(x => (x.Key, x.Value)).ToList();
    }

    public async Task Reset()
    {
        _config = GetDefaults();
        await Flush();
    }

    public async Task Flush()
    {
        var configPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".asuka/config.conf");
        
        _logger.LogInformation("Configuration path will be saved at: {ConfigPath}", configPath);

        var stringBuilder = new StringBuilder();
        foreach (var (key, value) in _config)
        {
            stringBuilder.Append($"{key}={value}\n");
        }

        try
        {
            await File.WriteAllTextAsync(configPath, stringBuilder.ToString());
        }
        catch (Exception e)
        {
            _logger.LogError("Cannot store configuration due to an exception: {@Exception}", e);
        }
    }
}
