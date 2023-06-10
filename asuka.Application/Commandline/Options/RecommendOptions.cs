using CommandLine;

namespace asuka.Application.Commandline.Options;

[Verb("recommend", HelpText = "Download recommendation from the gallery URL.")]
public record RecommendOptions : ICommonOptions
{
    [Option('i', "input",
        Required = true,
        HelpText = "Input Numeric Code")]
    public int Input { get; init; }
    public bool Pack { get; init; }
    public string Output { get; init; }
    public string Provider { get; init; }
}
