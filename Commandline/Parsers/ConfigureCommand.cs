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

    public async Task RunAsync(object options)
    {
        var opts = (ConfigureOptions)options;
        var validation = await _validator.ValidateAsync(opts);
        if (!validation.IsValid)
        {
            _consoleWriter.ValidationErrors(validation.Errors);
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
            _consoleWriter.WriteLine($"{opts.Key} = {configValue}");

            return;
        }

        if (opts.ListConfigMode)
        {
            var keyValuePairs = _configurationManager.GetAllValues();

            foreach (var (key, value) in keyValuePairs)
            {
                _consoleWriter.WriteLine($"{key} = {value}");
            }

            return;
        }

        if (opts.ResetConfig)
        {
            await _configurationManager.Reset();
        }
    }
}
