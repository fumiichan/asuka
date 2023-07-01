using System;
using asuka.Core.Output.Progress;
using ShellProgressBar;

namespace asuka.Application.Output.ProgressService.Providers;

public class ExternalProgressProvider : IProgressProvider
{
    private readonly IProgressBar _progress;

    public ExternalProgressProvider(int maxTicks, string title)
    {
        var config = new ProgressBarOptions
        {
            ForegroundColor = ConsoleColor.Yellow,
            ForegroundColorDone = ConsoleColor.Green,
            ProgressCharacter = '-'
        };

        _progress = new ProgressBar(maxTicks, title, config);
    }

    private ExternalProgressProvider(IProgressBar wrapper)
    {
        _progress = wrapper;
    }
    
    public IProgressProvider Spawn(int maxTicks, string title)
    {
        return new ExternalProgressProvider(_progress.Spawn(maxTicks, title, ProgressBarOptions.Default));
    }

    public void Tick()
    {
        _progress.Tick();
    }

    public void Tick(string message)
    {
        _progress.Tick(message);
    }

    public void Tick(int newTickCount)
    {
        _progress.Tick(newTickCount);
    }

    public void Tick(int newTickCount, string message)
    {
        _progress.Tick(newTickCount, message);
    }
}
