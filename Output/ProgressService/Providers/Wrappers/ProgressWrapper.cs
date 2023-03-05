using ShellProgressBar;

namespace asuka.Output.ProgressService.Providers.Wrappers;

public class ProgressWrapper : IProgressWrapper
{
    private readonly IProgressBar _progress;
    
    /// <summary>
    /// Initialize a Progress Wrapper with IProgressBar instance
    /// </summary>
    /// <param name="progress"></param>
    private ProgressWrapper(IProgressBar progress)
    {
        _progress = progress;
    }

    /// <summary>
    /// Initialize a Progress Wrapper with ProgressBar required parameters
    /// </summary>
    /// <param name="maxTicks"></param>
    /// <param name="title"></param>
    /// <param name="options"></param>
    public ProgressWrapper(int maxTicks, string title, ProgressBarOptions options)
    {
        _progress = new ProgressBar(maxTicks, title, options);
    }

    public IProgressWrapper Spawn(int maxTicks, string title, object options)
    {
        return new ProgressWrapper(_progress.Spawn(maxTicks, title, (ProgressBarOptions)options));
    }

    public void Tick(int newTickCount, string message = null)
    {
        _progress.Tick(newTickCount, message);
    }

    public void Tick(string message = null)
    {
        _progress.Tick(message);
    }
}
