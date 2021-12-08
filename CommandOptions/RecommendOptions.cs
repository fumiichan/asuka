using CommandLine;

namespace asuka.CommandOptions;

[Verb("recommend", HelpText = "Download recommendation from the gallery URL.")]
public record RecommendOptions : ICommonOptions, IRequiresInputOption
{
    public int Input { get; init; }
    public bool Pack { get; init; }
    public string Output { get; init; }
}
