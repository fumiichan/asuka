namespace asuka.Core.Events;

public abstract class ProgressEmittable
{
    private EventHandler<ProgressEvent> _progressEvent;
    
    protected void OnProgressEvent(ProgressEvent e)
    {
        _progressEvent?.Invoke(this, e);
    }

    public void HandleProgress(Action<ProgressEvent> e)
    {
        _progressEvent += (_, @event) =>
        {
            e(@event);
        };
    }
}
