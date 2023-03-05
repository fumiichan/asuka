using System;

namespace asuka.Output.ProgressService.Providers;

public class TextProgressProvider : IProgressProvider
{
    private readonly Guid _id;
    private int _maxTick = 0;
    private int _progress = 0;
    private readonly string _title;
    private readonly bool _isChild;
    private readonly int _parentId;

    public TextProgressProvider(int maxTick, string title)
    {
        _maxTick = maxTick;
        _title = title;
        _isChild = false;
        _id = Guid.NewGuid();
    }

    private TextProgressProvider(int maxTick, string title, int parentId)
    {
        _maxTick = maxTick;
        _title = title;
        _parentId = parentId;
        _isChild = true;
        _id = Guid.NewGuid();
    }

    public IProgressProvider Spawn(int maxTicks, string title, object options)
    {
        return new TextProgressProvider(maxTicks, title, _parentId);
    }

    public void Tick(string message = null)
    {
        _progress += 1;
        if (_isChild)
        {
            Console.WriteLine($"[Progress][Parent: {_parentId}] {message ?? _title} : {_progress} out of {_maxTick}");
            return;
        }
        
        Console.WriteLine($"[Progress] {message ?? _title} : {_progress} out of {_maxTick}");
    }

    public void Tick(int newTickCount, string message = null)
    {
        _maxTick = newTickCount;
        Tick(message);
    }
}
