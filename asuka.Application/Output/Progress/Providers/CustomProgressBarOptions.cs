using System;
using ShellProgressBar;

namespace asuka.Application.Output.Progress.Providers;

public static class CustomProgressBarOptions
{
    public static ProgressBarOptions Options => new ProgressBarOptions
    {
        ForegroundColor = ConsoleColor.Yellow,
        ForegroundColorDone = ConsoleColor.Green,
        ProgressCharacter = '-'
    };
}
