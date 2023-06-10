using System.Collections.Generic;
using CommandLine;

namespace asuka.Application.Commandline.Options;

[Verb("series", HelpText = "Construct series")]
public record SeriesCreatorCommandOptions: ICommonOptions
{
    [Option('a', "array", HelpText = "Create a seres from series of ids", Required = true)]
    public IEnumerable<string> FromList { get; set; }

    public bool Pack { get; init; }
    public string Output { get; init; }
    public string Provider { get; init; }
}
