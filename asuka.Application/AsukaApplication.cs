using System.Collections.Generic;
using System.Threading.Tasks;
using asuka.Application.Commandline;
using asuka.Application.Commandline.Options;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace asuka.Application;

public class AsukaApplication
{
    private readonly ICommandLineParserFactory _command;
    private readonly ILogger _logger;

    public AsukaApplication(ICommandLineParserFactory command, ILogger logger)
    {
        _command = command;
        _logger = logger;
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
            _ => Task.FromResult(1));
    }

    private async Task RunCommand(object opts, CommandLineParserTokens token)
    {
        var service = _command.GetInstance(token);
        await service.Run(opts);
        
        _logger.Log(LogLevel.Information, "Task Completed");
    }
}
