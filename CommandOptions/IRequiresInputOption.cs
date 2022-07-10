using CommandLine;

namespace asuka.CommandOptions;

public interface IRequiresInputOption
{
    [Option('i', "input",
        Required = true,
        HelpText = "Input Numeric Code")]
    int Input { get; init; }
}
