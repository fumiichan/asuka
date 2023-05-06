using System.Collections.Generic;
using System.Threading.Tasks;
using asuka.Application.Commandline;
using asuka.Application.Commandline.Options;
using asuka.Application.Output.Writer;
using CommandLine;

namespace asuka.Application;

public class AsukaApplication
{
    private readonly IConsoleWriter _console;
    private readonly ICommandLineParserFactory _command;

    public AsukaApplication(IConsoleWriter console, ICommandLineParserFactory command)
    {
        _console = console;
        _command = command;
    }

    public async Task RunAsync(IEnumerable<string> args)
    {
        var parser = Parser.Default
            .ParseArguments<GetOptions, RecommendOptions, SearchOptions, RandomOptions, FileCommandOptions, ConfigureOptions, SeriesCreatorCommandOptions, CookieConfigureOptions>(args);
        await parser.MapResult(
            async (GetOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Get); },
            async (RecommendOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Recommend); },
            async (SearchOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Search); },
            async (RandomOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Random); },
            async (FileCommandOptions opts) => { await RunCommand(opts, CommandLineParserTokens.File); },
            async (ConfigureOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Configure); },
            async (SeriesCreatorCommandOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Series); },
            async (CookieConfigureOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Cookie); },
            errors =>
            {
                foreach (var error in errors)
                {
                    _console.ErrorLine($"An error occured. Type: {error.Tag}");
                }

                return Task.FromResult(1);
            });
    }

    private async Task RunCommand(object opts, CommandLineParserTokens token)
    {
        var service = _command.GetInstance(token);
        await service.RunAsync(opts);
        
        _console.SuccessLine("Task completed.");
    }
}
