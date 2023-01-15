using CommandLine;
using CommandLine.Text;

namespace asuka.CommandOptions;

[Verb("random", HelpText = "Randomly pick a gallery.")]
public record RandomOptions : ICommonOptions
{
    public bool Pack { get; init; }
    public string Output { get; init; }
    public bool UseTachiyomiLayout { get; init; }
}
