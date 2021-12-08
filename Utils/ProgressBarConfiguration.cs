using System;
using ShellProgressBar;

namespace asuka.Utils;

public static class ProgressBarConfiguration
{
    public static readonly ProgressBarOptions BarOption = new()
    {
        ForegroundColor = ConsoleColor.Yellow,
        ForegroundColorDone = ConsoleColor.Green,
        ProgressCharacter = '-'
    };

    public static readonly ProgressBarOptions CompressOption = new()
    {
        ForegroundColor = ConsoleColor.DarkRed,
        ForegroundColorDone = ConsoleColor.Green,
        ProgressCharacter = '#'
    };
}
