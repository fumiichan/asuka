using CommandLine;

namespace asuka.Commandline.Options;

#nullable disable
[Verb("cookie", HelpText = "Configure cookies and User Agent")]
public record CookieConfigureOptions
{
    [Option('c', "cookie", HelpText = "Read cookies from a text file dump", Required = true)]
    public string CookieFile { get; init; }
    
    [Option('u', "userAgent", HelpText = "Set user agent", Required = true)]
    public string UserAgent { get; init; }
}
