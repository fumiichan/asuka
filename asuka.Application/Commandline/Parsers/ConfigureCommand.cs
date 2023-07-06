using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Configuration;
using asuka.Application.Utilities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

public class ConfigureCommand : ICommandLineParser
{
    private readonly IConfigManager _configManager;
    private readonly IValidator<ConfigureOptions> _validator;
    private readonly ILogger _logger;

    public ConfigureCommand(IValidator<ConfigureOptions> validator, IConfigManager configManager, ILogger logger)
    {
        _configManager = configManager;
        _logger = logger;
        _validator = validator;
    }

    public async Task Run(object options)
    {
        var opts = (ConfigureOptions)options;
        var validation = await _validator.ValidateAsync(opts);
        if (!validation.IsValid)
        {
            validation.Errors.PrintErrors(_logger);
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
            _logger.LogInformation($"{opts.Key} = {configValue}");

            return;
        }

        if (opts.ListConfigMode)
        {
            var keyValuePairs = _configManager.GetAllValues();

            foreach (var (key, value) in keyValuePairs)
            {
                _logger.LogInformation($"{key} = {value}");
            }

            return;
        }

        if (opts.ResetConfig)
        {
            await _configManager.Reset();
        }
    }
}
