using asuka.Application.Configuration;
using asuka.Application.Output.Progress.Providers;

namespace asuka.Application.Output.Progress;

public class ProgressProviderFactory : IProgressProviderFactory
{
    private readonly IConfigManager _config;

    public ProgressProviderFactory(IConfigManager config)
    {
        _config = config;
    }
    
    public IProgressProvider Create(int maxTicks, string title)
    {
        return _config.GetValue("tui.progress") switch
        {
            "text" => new TextProgressBar(maxTicks, title),
            "stealth" => new StealthProgressBar(),
            _ => new CustomProgressBar(maxTicks, title)
        };
    }
}
