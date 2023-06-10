using System;
using System.Collections.Generic;
using System.Linq;
using asuka.Application.Commandline.Parsers;

namespace asuka.Application.Commandline;

public class CommandLineParserFactory : ICommandLineParserFactory
{
    private readonly IEnumerable<ICommandLineParser> _parsers;

    public CommandLineParserFactory(IEnumerable<ICommandLineParser> parsers)
    {
        _parsers = parsers;
    }

    public ICommandLineParser GetInstance(CommandLineParserTokens token)
    {
        return token switch
        {
            CommandLineParserTokens.Configure => GetService(typeof(ConfigureCommand)),
            CommandLineParserTokens.File => GetService(typeof(FileCommandService)),
            CommandLineParserTokens.Get => GetService(typeof(GetCommandService)),
            CommandLineParserTokens.Random => GetService(typeof(RandomCommandService)),
            CommandLineParserTokens.Recommend => GetService(typeof(RecommendCommandService)),
            CommandLineParserTokens.Search => GetService(typeof(SearchCommandService)),
            CommandLineParserTokens.Series => GetService(typeof(SeriesCreatorCommandService)),
            _ => throw new ArgumentOutOfRangeException(nameof(token), token, null)
        };
    }

    private ICommandLineParser GetService(Type type)
    {
        return _parsers.FirstOrDefault(x => x.GetType() == type)!;
    }
}
