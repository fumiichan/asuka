using System.Collections.Generic;
using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Commandline.Parsers;
using CommandLine;
using asuka.Output.Writer;

namespace asuka;

public class AsukaApplication
{
    private readonly IGetCommandService _getCommand;
    private readonly IRecommendCommandService _recommendCommand;
    private readonly ISearchCommandService _searchCommand;
    private readonly IRandomCommandService _randomCommand;
    private readonly IFileCommandService _fileCommand;
    private readonly IConsoleWriter _console;
    private readonly IConfigureCommand _configureCommand;

    public AsukaApplication(
        IGetCommandService getCommand,
        IRecommendCommandService recommendCommand,
        IConsoleWriter console,
        ISearchCommandService searchCommand,
        IRandomCommandService randomCommand,
        IFileCommandService fileCommand,
        IConfigureCommand configureCommand)
    {
        _getCommand = getCommand;
        _recommendCommand = recommendCommand;
        _console = console;
        _searchCommand = searchCommand;
        _randomCommand = randomCommand;
        _fileCommand = fileCommand;
        _configureCommand = configureCommand;
    }

    public async Task RunAsync(IEnumerable<string> args)
    {
        var parser = Parser.Default
            .ParseArguments<GetOptions, RecommendOptions, SearchOptions, RandomOptions, FileCommandOptions, ConfigureOptions>(args);
        await parser.MapResult(
            async (GetOptions opts) => { await _getCommand.RunAsync(opts); },
            async (RecommendOptions opts) => { await _recommendCommand.RunAsync(opts); },
            async (SearchOptions opts) => { await _searchCommand.RunAsync(opts); },
            async (RandomOptions opts) => { await _randomCommand.RunAsync(opts); },
            async (FileCommandOptions opts) => { await _fileCommand.RunAsync(opts); },
            async (ConfigureOptions opts) => { await _configureCommand.RunAsync(opts); },
            _ => Task.FromResult(1));
        _console.SuccessLine("Task completed.");
    }
}
