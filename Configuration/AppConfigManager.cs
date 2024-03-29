﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace asuka.Configuration;

public class AppConfigManager : IAppConfigManager
{
    private Dictionary<string, string> _config;

    public AppConfigManager()
    {
        var configRoot = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".asuka");
        var configPath = Path.Join(configRoot, "config.conf");

        if (!File.Exists(configPath))
        {
            Directory.CreateDirectory(configRoot);
            _config = GetDefaults();

            return;
        }

        var data = File.ReadAllText(configPath, Encoding.UTF8);
        _config = ReadConfiguration(data);
    }

    private Dictionary<string, string> GetDefaults()
    {
        return new Dictionary<string, string>
        {
            {
                "colors.theme", "dark"
            },
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
            if (!dict.ContainsKey(value.Key))
            {
                dict.Add(value.Key, value.Value);
            }
        }

        return dict;
    }

    public void SetValue(string key, string value)
    {
        _config[key] = value;
    }

    public string GetValue(string key)
    {
        return _config.GetValueOrDefault(key) ?? "";
    }

    public IReadOnlyList<(string, string)> GetAllValues()
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

        var stringBuilder = new StringBuilder();
        foreach (var (key, value) in _config)
        {
            stringBuilder.Append($"{key}={value}\n");
        }

        await File.WriteAllTextAsync(configPath, stringBuilder.ToString());
    }
}
