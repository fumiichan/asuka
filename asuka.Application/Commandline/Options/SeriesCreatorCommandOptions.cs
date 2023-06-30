using System.Collections.Generic;
using CommandLine;

namespace asuka.Application.Commandline.Options;

[Verb("series", HelpText = "Construct series")]
public record SeriesCreatorCommandOptions: ICommonOptions
{
    [Option('a', "array", HelpText = "Create a seres from series of ids", Required = true)]
    public IEnumerable<string> FromList { get; init; }
    
    [Option("startOffset", HelpText = "Change number where chapter numbering starts", Default = 1)]
    public int StartOffset { get; init; }
    
    [Option("noMeta", HelpText = "Disable metadata writing")]
    public bool DisableMetaWriting { get; init; }

    public bool Pack { get; init; }
    
    public string Output { get; init; }
    
    [Option("provider", HelpText = "Select default provider to use if not specified on URL")]
    public string Provider { get; init; }
}
