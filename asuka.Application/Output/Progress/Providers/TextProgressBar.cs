using System;

namespace asuka.Application.Output.Progress.Providers;

public class TextProgressBar : IProgressProvider
{
    private bool _disposed;
    private readonly int _spacing;
    private int _progress;
    private int _maxTicks;
    private string _title;

    public TextProgressBar(int maxTicks, string title)
    {
        _maxTicks = maxTicks;
        _title = title;
    }

    private TextProgressBar(int spacing, int maxTicks, string title)
    {
        _spacing = spacing;
        _maxTicks = maxTicks;
        _title = title;
    }

    public void Tick()
    {
        _progress++;
        var print = $"{_title} : {_progress} out of {_maxTicks}";
        Console.WriteLine(print.PadLeft(print.Length + _spacing), ' ');
    }

    public void Tick(int newMaxTicks)
    {
        _maxTicks = newMaxTicks;
        Tick();
    }

    public void Tick(string message)
    {
        _title = message;
        Tick();
    }

    public void Tick(int newMaxTicks, string message)
    {
        _maxTicks = newMaxTicks;
        _title = message;
        Tick();
    }

    public void Close()
    {
        if (IsClosed())
        {
            return;
        }
        _disposed = true;
        
        Console.WriteLine("\n\n");
    }

    public bool IsClosed()
    {
        return _disposed;
    }

    public IProgressProvider Spawn(int maxTicks, string message)
    {
        return IsClosed() ? null : new TextProgressBar(_spacing + 2, maxTicks, message);
    }
}
