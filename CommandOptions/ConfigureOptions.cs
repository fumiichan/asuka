using CommandLine;

namespace asuka.CommandOptions;

[Verb("config", HelpText = "Configure the client")]
public record ConfigureOptions
{
    [Option('c', "setDefaultCookies", HelpText = "Sets/Updates the cookies")]
    public string SetDefaultCookies { get; init; }
}
