using System.Collections.Generic;
using System.Drawing;
using asuka.Configuration;
using FluentValidation.Results;
using Console = Colorful.Console;

namespace asuka.Output.Writer;

public class ConsoleWriter : IConsoleWriter
{
    private readonly IAppConfigManager _appConfigManager;

    public ConsoleWriter(IAppConfigManager appConfigManager)
    {
        _appConfigManager = appConfigManager;
    }

    private Color GetColor(Color forWhiteTheme, Color forDarkTheme)
    {
        return _appConfigManager.GetValue("color.theme") == "dark" ? forDarkTheme : forWhiteTheme;
    }

    public void WriteLine(object message)
    {
        Console.WriteLine(message, GetColor(Color.Red, Color.Aqua));
    }

    public void WarningLine(object message)
    {
        Console.WriteLine(message, GetColor(Color.Blue, Color.Yellow));
    }

    public void ErrorLine(string message)
    {
        Console.WriteLine(message, GetColor(Color.Teal, Color.IndianRed));
    }

    public void SuccessLine(string message)
    {
        Console.WriteLine(message, GetColor(Color.Purple, Color.Green));
    }

    public void ValidationErrors(IEnumerable<ValidationFailure> errors)
    {
        foreach (var error in errors)
        {
            ErrorLine($"{error.ErrorCode}: {error.ErrorMessage}");
        }
    }
}
