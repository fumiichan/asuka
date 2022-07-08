using System.Threading.Tasks;
using asuka.CommandOptions;
using asuka.Configuration;

namespace asuka.CommandParsers;

public class ConfigureCommand : IConfigureCommand
{
    public async Task RunAsync(ConfigureOptions opts)
    {
        var config = new ConfigurationManager();
        
        if (!string.IsNullOrEmpty(opts.SetDefaultCookies))
        {
            await config.SetCookies(opts.SetDefaultCookies);
        }

        if (!string.IsNullOrEmpty(opts.SetUserAgent))
        {
            await config.SetUserAgent(opts.SetUserAgent);
        }
    }
}
