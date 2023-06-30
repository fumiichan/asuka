using CommandLine;

namespace asuka.Application.Commandline.Options;

[Verb("recommend", HelpText = "Download recommendation from the gallery URL.")]
public record RecommendOptions : ICommonOptions
{
    [Option('i', "input",
        Required = true,
        HelpText = "Input Numeric Code")]
    public string Input { get; init; }
    public bool Pack { get; init; }
    public string Output { get; init; }

    [Option("provider", HelpText = "Select default provider to use if not specified on URL")]
    public string Provider { get; init; }
}
