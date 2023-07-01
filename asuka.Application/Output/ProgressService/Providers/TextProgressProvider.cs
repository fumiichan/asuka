using System;
using asuka.Core.Output.Progress;

namespace asuka.Application.Output.ProgressService.Providers;

public class TextProgressProvider : IProgressProvider
{
    private int _maxTick;
    private int _progress;
    private readonly string _title;
    private readonly bool _isChild;
    private readonly int _parentId;

    public TextProgressProvider(int maxTick, string title)
    {
        _maxTick = maxTick;
        _title = title;
        _isChild = false;
    }

    private TextProgressProvider(int maxTick, string title, int parentId)
    {
        _maxTick = maxTick;
        _title = title;
        _parentId = parentId;
        _isChild = true;
    }

    public IProgressProvider Spawn(int maxTicks, string title)
    {
        return new TextProgressProvider(maxTicks, title, _parentId);
    }

    public void Tick()
    {
        Tick(null);
    }

    public void Tick(string message)
    {
        _progress += 1;
        if (_isChild)
        {
            Console.WriteLine($"[Progress][Parent: {_parentId}] {message ?? _title} : {_progress} out of {_maxTick}");
            return;
        }
        
        Console.WriteLine($"[Progress] {message ?? _title} : {_progress} out of {_maxTick}");
    }

    public void Tick(int newTickCount)
    {
        Tick(newTickCount, null);
    }

    public void Tick(int newTickCount, string message)
    {
        _maxTick = newTickCount;
        Tick(message);
    }
}
