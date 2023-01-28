using System.Text.Json;
using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Configuration;
using asuka.Output.Writer;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class ConfigureCommand : ICommandLineParser
{
    private readonly IConfigurationManager _configurationManager;
    private readonly IConsoleWriter _consoleWriter;
    private readonly IValidator<ConfigureOptions> _validator;

    public ConfigureCommand(IValidator<ConfigureOptions> validator, IConfigurationManager configurationManager, IConsoleWriter consoleWriter)
    {
        _configurationManager = configurationManager;
        _consoleWriter = consoleWriter;
        _validator = validator;
    }

    private void ListAllConfigurationValues()
    {
        _consoleWriter.WriteLine($"Cookies: {JsonSerializer.Serialize(_configurationManager.Values.Cookies)}");
        _consoleWriter.WriteLine($"User Agent: {_configurationManager.Values.UserAgent}");
        _consoleWriter.WriteLine($"Theme: {_configurationManager.Values.ConsoleTheme}");
        _consoleWriter.WriteLine($"UseTachiyomiLayout: {_configurationManager.Values.UseTachiyomiLayout}");
    }
    
    public async Task RunAsync(object options)
    {
        var opts = (ConfigureOptions)options;
        var validation = await _validator.ValidateAsync(opts);
        if (!validation.IsValid)
        {
            _consoleWriter.ValidationErrors(validation.Errors);
            return;
        }
        
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

        if (!string.IsNullOrEmpty(opts.Theme))
        {
            await _configurationManager.ChangeColourThemeAsync(opts.Theme);
        }
    }
}
