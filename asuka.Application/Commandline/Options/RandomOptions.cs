using CommandLine;

namespace asuka.Application.Commandline.Options;

[Verb("random", HelpText = "Randomly pick a gallery.")]
public record RandomOptions : ICommonOptions
{
    public bool Pack { get; init; }
    
    public string Output { get; init; }
    
    [Option("provider",
        Required = true,
        HelpText = "Target provider to get a random result")]
    public string Provider { get; init; }
}
