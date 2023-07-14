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
    
    public IProgressProvider Create(int maxTicks, string message)
    {
        return _config.GetValue("tui.progress") switch
        {
            "text" => new TextProgressBar(maxTicks, message),
            "stealth" => new StealthProgressBar(),
            _ => new CustomProgressBar(maxTicks, message)
        };
    }
}
