using System.Collections.Generic;
using CommandLine;

namespace asuka.Application.Commandline.Options;

[Verb("get", HelpText = "Download a Single Gallery from URL.")]
public record GetOptions : ICommonOptions
{
    [Option('i', "input",
        Required = true,
        HelpText = "Input Numeric Code(s)")]
    public IEnumerable<string> Input { get; init; }

    [Option('r', "readonly",
        Default = false,
        Required = false,
        HelpText = "View the information only")]
    public bool ReadOnly { get; init; }

    public bool Pack { get; init; }
    public string Output { get; init; }
    public string Provider { get; init; }
}
