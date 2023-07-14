namespace asuka.Application.Output.ConsoleWriter;

public interface IConsoleWriter
{
    void Write(string message);
    void WriteInformation(string message);
    void WriteWarning(string message);
    void WriteError(string message);
}

