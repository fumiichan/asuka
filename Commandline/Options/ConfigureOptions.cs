using CommandLine;

namespace asuka.Commandline.Options;

#nullable disable
[Verb("config", HelpText = "Configure the client")]
public record ConfigureOptions
{
    [Option('s', "set", HelpText = "Set value")]
    public bool SetConfigMode { get; init; }
    
    [Option('r', "read", HelpText = "Read configuration")]
    public bool ReadConfigMode { get; init; }
    
    [Option('l', "list", HelpText = "List all configuration values")]
    public bool ListConfigMode { get; init; }
    
    [Option('k', "key", HelpText = "Configuration to set/read")]
    public string Key { get; init; }
    
    [Option('v', "value", HelpText = "New value")]
    public string Value { get; init; }
    
    [Option("reset", HelpText = "Reset configuration values")]
    public bool ResetConfig { get; init; }
}
