using System.Threading.Tasks;
using asuka.CommandOptions;
using asuka.Configuration;

namespace asuka.CommandParsers;

public class ConfigureCommand : IConfigureCommand
{
    private readonly IConfigurationManager _configurationManager;

    public ConfigureCommand(IConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }
    
    public async Task RunAsync(ConfigureOptions opts)
    {
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
