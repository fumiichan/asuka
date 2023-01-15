using CommandLine;

namespace asuka.CommandOptions;

public interface ICommonOptions
{
    [Option('p', "pack",
        Default = false,
        Required = false,
        HelpText = "Pack downloaded gallery into single CBZ archive")]
    bool Pack { get; init; }

    [Option('o', "output",
        Required = false,
        HelpText = "Destination path for gallery download")]
    string Output { get; init; }

    [Option("useTachiyomiLayout",
        Required = false,
        HelpText = "Uses Tachiyomi Folder Structure for Download")]
    bool UseTachiyomiLayout { get; init; }
}
