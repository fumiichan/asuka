namespace asuka.Core.Events;

public class ProgressEvent : EventArgs
{
    public readonly string Message;

    public ProgressEvent(string message)
    {
        Message = message;
    }
}
