using asuka.Application.Output.ProgressService.Providers;
using asuka.Core.Output.Progress;

namespace asuka.Application.Output.ProgressService;

public static class ProgressProviderFactory
{
    public static IProgressProvider GetProvider(string tuiOption, int maxTicks, string title)
    {
        return tuiOption switch
        {
            "stealth" => new VoidProgressProvider(),
            "text" => new TextProgressProvider(maxTicks, title),
            _ => new ExternalProgressProvider(maxTicks, title)
        };
    }
}
