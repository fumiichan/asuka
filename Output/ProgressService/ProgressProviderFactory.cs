using asuka.Core.Output.Progress;
using asuka.Output.ProgressService.Providers;

namespace asuka.Output.ProgressService;

public static class ProgressProviderFactory
{
    public static IProgressProvider GetProvider(string tuiOption, int maxTicks, string title, object options)
    {
        return tuiOption switch
        {
            "stealth" => new VoidProgressProvider(),
            "text" => new TextProgressProvider(maxTicks, title),
            _ => new ExternalProgressProvider(maxTicks, title, options)
        };
    }
}
