namespace asuka.Core.Events;

public class ProgressEventArgs : EventArgs
{
    public readonly string Message;

    public ProgressEventArgs(string message)
    {
        Message = message;
    }
}
