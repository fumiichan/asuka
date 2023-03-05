using asuka.Output.ProgressService.Providers.Wrappers;
using ShellProgressBar;

namespace asuka.Output.ProgressService.Providers;

public class ExternalProgressProvider : IProgressProvider
{
    private readonly IProgressWrapper _progress;

    public ExternalProgressProvider(int maxTicks, string title, object options)
    {
        _progress = new ProgressWrapper(maxTicks, title, (ProgressBarOptions)options);
    }

    private ExternalProgressProvider(IProgressWrapper wrapper)
    {
        _progress = wrapper;
    }
    
    public IProgressProvider Spawn(int maxTicks, string title, object options)
    {
        return new ExternalProgressProvider(_progress.Spawn(maxTicks, title, options));
    }

    public void Tick(string message = null)
    {
        _progress.Tick(message);
    }

    public void Tick(int newTickCount, string message = null)
    {
        _progress.Tick(newTickCount, message);
    }
}
