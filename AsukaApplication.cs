using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using asuka.CommandOptions;
using asuka.CommandParsers;
using asuka.Output;
using asuka.Services;

namespace asuka
{
    public class AsukaApplication
    {
        private readonly IGetCommandService _getCommand;
        private readonly IRecommendCommandService _recommendCommand;
        private readonly ISearchCommandService _searchCommand;
        private readonly IRandomCommandService _randomCommand;
        private readonly IConsoleWriter _console;

        public AsukaApplication(
            IGetCommandService getCommand,
            IRecommendCommandService recommendCommand,
            IConsoleWriter console,
            ISearchCommandService searchCommand,
            IRandomCommandService randomCommand)
        {
            _getCommand = getCommand;
            _recommendCommand = recommendCommand;
            _console = console;
            _searchCommand = searchCommand;
            _randomCommand = randomCommand;
        }

        public async Task RunAsync(IEnumerable<string> args)
        {
            try
            {
                var parser = new Parser(with => with.HelpWriter = null)
                    .ParseArguments<GetOptions, RecommendOptions, SearchOptions, RandomOptions>(args);
                await parser.MapResult(
                    async (GetOptions opts) => { await _getCommand.RunAsync(opts); },
                    async (RecommendOptions opts) => { await _recommendCommand.RunAsync(opts); },
                    async (SearchOptions opts) => { await _searchCommand.RunAsync(opts); },
                    async (RandomOptions opts) => { await _randomCommand.RunAsync(opts); },
                    errors =>
                    {
                        var helpText = HelpText.AutoBuild(parser, h =>
                        {
                            h.AddEnumValuesToHelpText = true;
                            return h;
                        }, e => e);

                        Console.WriteLine(helpText);
                        return Task.FromResult(1);
                    });
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
}