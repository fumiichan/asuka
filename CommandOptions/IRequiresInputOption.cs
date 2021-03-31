using CommandLine;

namespace asuka.CommandOptions
{
    public interface IRequiresInputOption
    {
        [Option('i', "input",
            Required = true,
            HelpText = "Input URL")]
        int Input { get; init; }
    }
}