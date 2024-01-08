using asuka.Core.Output.Progress;
using ShellProgressBar;

namespace asuka.Output.Progress;

public class ShellProgressBarWrapper : IProgressProvider
{
    private readonly IProgressBar _progress;

    public ShellProgressBarWrapper(int maxTicks, string message)
    {
        _progress = new ProgressBar(maxTicks, message, ShellProgressBarOptions.Options);
    }

    private ShellProgressBarWrapper(IProgressBar progress, int maxTicks, string message)
    {
        _progress = progress.Spawn(maxTicks, message, ShellProgressBarOptions.Options);
    }

    public void Tick()
    {
        _progress.Tick();
    }

    public void Tick(int newMaxTicks)
    {
        _progress.Tick(newMaxTicks);
    }

    public void Tick(string message)
    {
        _progress.Tick(message);
    }

    public void Tick(int newMaxTicks, string message)
    {
        _progress.Tick(newMaxTicks, message);
    }

    public void Stop()
    {
        var progress = _progress.AsProgress<float>();
        progress.Report(1);
    }

    public void Stop(string message)
    {
        _progress.Message = message;
        Stop();
    }

    public IProgressProvider Spawn(int maxTicks, string message)
    {
        return new ShellProgressBarWrapper(_progress, maxTicks, message);
    }
}
