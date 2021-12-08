using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using asuka.CommandOptions;
using asuka.CommandParsers;
using asuka.Output;

namespace asuka;

public class AsukaApplication
{
    private readonly IGetCommandService _getCommand;
    private readonly IRecommendCommandService _recommendCommand;
    private readonly ISearchCommandService _searchCommand;
    private readonly IRandomCommandService _randomCommand;
    private readonly IFileCommandService _fileCommand;
    private readonly IConsoleWriter _console;

    public AsukaApplication(
        IGetCommandService getCommand,
        IRecommendCommandService recommendCommand,
        IConsoleWriter console,
        ISearchCommandService searchCommand,
        IRandomCommandService randomCommand,
        IFileCommandService fileCommand)
    {
        _getCommand = getCommand;
        _recommendCommand = recommendCommand;
        _console = console;
        _searchCommand = searchCommand;
        _randomCommand = randomCommand;
        _fileCommand = fileCommand;
    }

    public async Task RunAsync(IEnumerable<string> args)
    {
        try
        {
            var parser = Parser.Default
                .ParseArguments<GetOptions, RecommendOptions, SearchOptions, RandomOptions, FileCommandOptions>(args);
            await parser.MapResult(
                async (GetOptions opts) => { await _getCommand.RunAsync(opts); },
                async (RecommendOptions opts) => { await _recommendCommand.RunAsync(opts); },
                async (SearchOptions opts) => { await _searchCommand.RunAsync(opts); },
                async (RandomOptions opts) => { await _randomCommand.RunAsync(opts); },
                async (FileCommandOptions opts) => { await _fileCommand.RunAsync(opts); },
                _ => Task.FromResult(1));
            _console.SuccessLine("Task completed.");
        }
        catch (Exception err)
        {
            _console.ErrorLine($"An exception occured. Error: {err.Message}");
            _console.ErrorLine("Full Error:");
            _console.WriteLine(err);
        }
    }
}
