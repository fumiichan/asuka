using CommandLine;

namespace asuka.Commandline.Options;

[Verb("config", HelpText = "Configure the client")]
public record ConfigureOptions
{
    [Option('c', "setDefaultCookies", HelpText = "Sets/Updates the cookies")]
    public string SetDefaultCookies { get; init; }
    
    [Option('u', "setUserAgent", HelpText = "Set default User Agent")]
    public string SetUserAgent { get; init; }
    
    [Option('l', "useTachiyomiLayout", HelpText = "Toggle Tachiyomi Layout")]
    public string UseTachiyomiLayoutToggle { get; init; }
    
    [Option('t', "theme", HelpText = "Use a colour scheme according to your console background. Supported values are light and dark")]
    public string Theme { get; init; }
    
    [Option("reset", HelpText = "Reset configuration values")]
    public bool ResetConfig { get; init; }
    
    [Option("list", HelpText = "List values of configuration")]
    public bool JustList { get; init; }
}
