using asuka.Application.Output.Progress.Providers;
using asuka.Application.Services.Configuration;

namespace asuka.Application.Output.Progress;

public class ProgressProviderFactory : IProgressProviderFactory
{
    private readonly AsukaConfiguration _config;

    public ProgressProviderFactory(AsukaConfiguration config)
    {
        _config = config;
    }
    
    public IProgressProvider Create(int maxTicks, string message)
    {
        return _config.ProgressType switch
        {
            ProgressTypes.Text => new TextProgressBar(maxTicks, message),
            ProgressTypes.Stealth => new StealthProgressBar(),
            _ => new CustomProgressBar(maxTicks, message)
        };
    }
}
