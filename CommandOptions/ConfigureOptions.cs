using CommandLine;

namespace asuka.CommandOptions;

[Verb("config", HelpText = "Configure the client")]
public record ConfigureOptions
{
    [Option('c', "setDefaultCookies", HelpText = "Sets/Updates the cookies")]
    public string SetDefaultCookies { get; init; }
    
    [Option('u', "setUserAgent", HelpText = "Set default User Agent")]
    public string SetUserAgent { get; init; }
    
    [Option('l', "useTachiyomiLayout", HelpText = "Toggle Tachiyomi Layout")]
    public string UseTachiyomiLayoutToggle { get; init; }
    
    [Option("list", HelpText = "List values of configuration")]
    public bool JustList { get; init; }
}
