using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Utilities;
using asuka.Core.Configuration;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

public class ConfigureCommand : ICommandLineParser
{
    private readonly IConfigurationManager _configurationManager;
    private readonly IValidator<ConfigureOptions> _validator;
    private readonly ILogger _logger;

    public ConfigureCommand(IValidator<ConfigureOptions> validator, IConfigurationManager configurationManager, ILogger logger)
    {
        _configurationManager = configurationManager;
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
            _configurationManager.SetValue(opts.Key, opts.Value);
            await _configurationManager.Flush();
            
            return;
        }

        if (opts.ReadConfigMode)
        {
            var configValue = _configurationManager.GetValue(opts.Key);
            _logger.LogInformation($"{opts.Key} = {configValue}");

            return;
        }

        if (opts.ListConfigMode)
        {
            var keyValuePairs = _configurationManager.GetAllValues();

            foreach (var (key, value) in keyValuePairs)
            {
                _logger.LogInformation($"{key} = {value}");
            }

            return;
        }

        if (opts.ResetConfig)
        {
            await _configurationManager.Reset();
        }
    }
}
