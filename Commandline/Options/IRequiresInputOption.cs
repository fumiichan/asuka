using CommandLine;

namespace asuka.Commandline.Options;

public interface IRequiresInputOption
{
    [Option('i', "input",
        Required = true,
        HelpText = "Input Numeric Code")]
    int Input { get; init; }
}
