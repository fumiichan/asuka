using System;

namespace asuka.Core.Output.Progress;

public class TextProgressBar : IProgressProvider
{
    private readonly int _spacing;
    private int _progress;
    private int _maxTicks;
    private string _title;
    private bool _stopped;

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
        if (_stopped)
        {
            return;
        }
        
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

    public void Stop()
    {
        _stopped = true;
        var print = $"{_title} : {_progress} out of {_maxTicks}";
        Console.WriteLine(print.PadLeft(print.Length + _spacing), ' ');
    }

    public void Stop(string message)
    {
        _stopped = true;
        _title = message;

        var print = $"{_title} : {_progress} out of {_maxTicks}";
        Console.WriteLine(print.PadLeft(print.Length + _spacing), ' ');
    }

    public IProgressProvider? Spawn(int maxTicks, string message)
    {
        if (_stopped)
        {
            return null;
        }

        return new TextProgressBar(_spacing + 2, maxTicks, message);
    }
}
