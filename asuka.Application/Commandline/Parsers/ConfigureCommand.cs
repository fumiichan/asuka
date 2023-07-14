using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Configuration;
using asuka.Application.Output.ConsoleWriter;
using asuka.Application.Utilities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

public class ConfigureCommand : ICommandLineParser
{
    private readonly IConfigManager _configManager;
    private readonly IValidator<ConfigureOptions> _validator;
    private readonly ILogger _logger;
    private readonly IConsoleWriter _console;

    public ConfigureCommand(
        IValidator<ConfigureOptions> validator,
        IConfigManager configManager,
        ILogger logger,
        IConsoleWriter console)
    {
        _configManager = configManager;
        _logger = logger;
        _validator = validator;
        _console = console;
    }

    public async Task Run(object options)
    {
        var opts = (ConfigureOptions)options;
        _logger.LogInformation("ConfigureCommand called with args: {@Opts}", opts);
        
        var validation = await _validator.ValidateAsync(opts);
        if (!validation.IsValid)
        {
            _logger.LogError("ConfigureCommand failed on validation {@Errors}", validation.Errors);
            validation.Errors.PrintErrors(_console);
            return;
        }

        if (opts.SetConfigMode)
        {
            _configManager.SetValue(opts.Key, opts.Value);
            await _configManager.Flush();
            
            return;
        }

        if (opts.ReadConfigMode)
        {
            var configValue = _configManager.GetValue(opts.Key);
            _console.Write($"{opts.Key} = {configValue}");

            return;
        }

        if (opts.ListConfigMode)
        {
            var keyValuePairs = _configManager.GetAllValues();

            foreach (var (key, value) in keyValuePairs)
            {
                _console.Write($"{key} = {value}");
            }

            return;
        }

        if (opts.ResetConfig)
        {
            await _configManager.Reset();
        }
    }
}
