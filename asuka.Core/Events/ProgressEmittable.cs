namespace asuka.Core.Events;

public abstract class ProgressEmittable
{
    private EventHandler<ProgressEventArgs> _progressEvent;
    
    protected void OnProgressEvent(ProgressEventArgs e)
    {
        _progressEvent?.Invoke(this, e);
    }

    public void HandleProgress(Action<ProgressEventArgs> e)
    {
        _progressEvent += (_, @event) =>
        {
            e(@event);
        };
    }
}
