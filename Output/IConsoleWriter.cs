namespace asuka.Output
{
    public interface IConsoleWriter
    {
        void WriteLine(object message);
        void WarningLine(object message);
        void ErrorLine(string message);
        void SuccessLine(string message);
    }
}