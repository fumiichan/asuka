using System.Collections.Generic;
using System.Threading.Tasks;
using asuka.Application.Commandline;
using asuka.Application.Commandline.Options;
using asuka.Application.Output.ConsoleWriter;
using CommandLine;

namespace asuka.Application;

public class AsukaApplication
{
    private readonly ICommandLineParserFactory _command;
    private readonly IConsoleWriter _console;

    public AsukaApplication(ICommandLineParserFactory command, IConsoleWriter console)
    {
        _command = command;
        _console = console;
    }

    public async Task RunAsync(IEnumerable<string> args)
    {
        var parser = Parser.Default
            .ParseArguments<GetOptions, RecommendOptions, SearchOptions, RandomOptions, FileCommandOptions, ConfigureOptions, SeriesCreatorCommandOptions>(args);
        await parser.MapResult(
            async (GetOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Get); },
            async (RecommendOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Recommend); },
            async (SearchOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Search); },
            async (RandomOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Random); },
            async (FileCommandOptions opts) => { await RunCommand(opts, CommandLineParserTokens.File); },
            async (ConfigureOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Configure); },
            async (SeriesCreatorCommandOptions opts) => { await RunCommand(opts, CommandLineParserTokens.Series); },
            _ => Task.FromResult(1));
    }

    private async Task RunCommand(object opts, CommandLineParserTokens token)
    {
        var service = _command.GetInstance(token);
        await service.Run(opts);
        
        _console.WriteInformation("Task Completed");
    }
}
