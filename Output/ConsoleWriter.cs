using System.Drawing;
using Console = Colorful.Console;

namespace asuka.Output;

public class ConsoleWriter : IConsoleWriter
{
    public void WriteLine(object message)
    {
        Console.WriteLine(message, Color.Aqua);
    }

    public void WarningLine(object message)
    {
        Console.WriteLine(message, Color.Yellow);
    }

    public void ErrorLine(string message)
    {
        Console.WriteLine(message, Color.IndianRed);
    }

    public void SuccessLine(string message)
    {
        Console.WriteLine(message, Color.Green);
    }
}
