using CommandLine;

namespace asuka.Commandline.Options;

[Verb("get", HelpText = "Download a Single Gallery from URL.")]
public record GetOptions : ICommonOptions
{
    [Option('i', "input",
        Required = true,
        HelpText = "Input Numeric Code(s)")]
    public int[] Input { get; init; }

    [Option('r', "readonly",
        Default = false,
        Required = false,
        HelpText = "View the information only")]
    public bool ReadOnly { get; init; }

    public bool Pack { get; init; }
    public string Output { get; init; }
}
