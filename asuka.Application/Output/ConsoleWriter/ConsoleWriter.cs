using System;

namespace asuka.Application.Output.ConsoleWriter;

public class ConsoleWriter : IConsoleWriter
{
    private void ColourConsole(ConsoleColor color, string message)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }

    public void Write(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteInformation(string message)
    {
        ColourConsole(ConsoleColor.DarkCyan, message);
    }

    public void WriteWarning(string message)
    {
        ColourConsole(ConsoleColor.Yellow, message);
    }

    public void WriteError(string message)
    {
        ColourConsole(ConsoleColor.Red, message);
    }
}
