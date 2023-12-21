using asuka.Configuration;
using asuka.Core.Output.Progress;

namespace asuka.Output.Progress;

public class ProgressFactory : IProgressFactory
{
    private readonly IAppConfigManager _config;

    public ProgressFactory(IAppConfigManager config)
    {
        _config = config;
    }
    
    public IProgressProvider Create(int maxTicks, string message)
    {
        return _config.GetValue("tui.progress") switch
        {
            "stealth" => Create(ProgressTypes.Stealth, maxTicks, message),
            "text" => Create(ProgressTypes.Text, maxTicks, message),
            _ => Create(ProgressTypes.Progress, maxTicks, message)
        };
    }

    public IProgressProvider Create(ProgressTypes type, int maxTicks, string message)
    {
        if (type == ProgressTypes.Stealth)
        {
            return new StealthProgressBar();
        }

        if (type == ProgressTypes.Text)
        {
            return new TextProgressBar(maxTicks, message);
        }

        return new ShellProgressBarWrapper(maxTicks, message);
    }
}