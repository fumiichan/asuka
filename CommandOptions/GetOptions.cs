using CommandLine;

namespace asuka.CommandOptions
{
    [Verb("get", HelpText = "Download a Single Gallery from URL.")]
    public record GetOptions : IRequiresInputOption, ICommonOptions
    {
        public string Input { get; init; }
        
        [Option('r', "readonly",
            Default = false,
            Required = false,
            HelpText = "View the information only")]
        public bool ReadOnly { get; init; }

        public bool Pack { get; init; }
        public string Output { get; init; }
    }
}