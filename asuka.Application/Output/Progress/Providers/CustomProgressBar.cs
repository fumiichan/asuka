using System;
using ShellProgressBar;

namespace asuka.Application.Output.Progress.Providers;

public class CustomProgressBar : IProgressProvider
{
    private readonly IProgressBar _progress;
    private bool _disposed;

    public CustomProgressBar(int maxTicks, string message)
    {
        _progress = new ProgressBar(maxTicks, message, CustomProgressBarOptions.Options);
    }

    private CustomProgressBar(IProgressBar progress, int maxTicks, string message)
    {
        _progress = progress.Spawn(maxTicks, message, CustomProgressBarOptions.Options);
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

    public void Close()
    {
        if (_disposed)
        {
            return;
        }

        _progress?.Dispose();
        _disposed = true;
        
        Console.WriteLine("\n\n");
    }

    public bool IsClosed()
    {
        return _disposed;
    }

    public IProgressProvider Spawn(int maxTicks, string message)
    {
        return new CustomProgressBar(_progress, maxTicks, message);
    }
}
