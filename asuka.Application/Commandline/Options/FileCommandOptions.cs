using CommandLine;

namespace asuka.Application.Commandline.Options;

[Verb("file", HelpText = "Download galleries from text file")]
public record FileCommandOptions : ICommonOptions
{
    [Option('f', "file",
        Required = true,
        HelpText = "Path to text file to read.")]
    public string FilePath { get; init; }

    public bool Pack { get; init; }
    
    public string Output { get; init; }

    [Option("provider", HelpText = "Select default provider to use if not specified on URL")]
    public string Provider { get; init; }
}
