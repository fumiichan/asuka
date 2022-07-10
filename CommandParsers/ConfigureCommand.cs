using System.Text.Json.Serialization;
using System.Threading.Tasks;
using asuka.CommandOptions;
using asuka.Configuration;
using asuka.Output;
using Newtonsoft.Json;

namespace asuka.CommandParsers;

public class ConfigureCommand : IConfigureCommand
{
    private readonly IConfigurationManager _configurationManager;
    private readonly IConsoleWriter _consoleWriter;

    public ConfigureCommand(IConfigurationManager configurationManager, IConsoleWriter consoleWriter)
    {
        _configurationManager = configurationManager;
        _consoleWriter = consoleWriter;
    }

    private void ListAllConfigurationValues()
    {
        _consoleWriter.WriteLine($"Cookies: {JsonConvert.SerializeObject(_configurationManager.Values.Cookies)}");
        _consoleWriter.WriteLine($"User Agent: {_configurationManager.Values.UserAgent}");
        _consoleWriter.WriteLine($"UseTachiyomiLayout: {_configurationManager.Values.UseTachiyomiLayout}");
    }
    
    public async Task RunAsync(ConfigureOptions opts)
    {
        if (opts.JustList)
        {
            ListAllConfigurationValues();
            return;
        }

        if (opts.ResetConfig)
        {
            await _configurationManager.ResetAsync();
            _consoleWriter.SuccessLine("Configuration has been reset.");
            return;
        }
        
        if (!string.IsNullOrEmpty(opts.SetDefaultCookies))
        {
            await _configurationManager.SetCookiesAsync(opts.SetDefaultCookies);
        }

        if (!string.IsNullOrEmpty(opts.SetUserAgent))
        {
            await _configurationManager.SetUserAgentAsync(opts.SetUserAgent);
        }

        if (opts.UseTachiyomiLayoutToggle == bool.FalseString || opts.UseTachiyomiLayoutToggle == bool.TrueString)
        {
            await _configurationManager.ToggleTachiyomiLayoutAsync(bool.Parse(opts.UseTachiyomiLayoutToggle));
        }
    }
}
