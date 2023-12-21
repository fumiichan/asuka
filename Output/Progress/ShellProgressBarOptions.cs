using System;
using ShellProgressBar;

namespace asuka.Output.Progress;

public class ShellProgressBarOptions
{
    public static ProgressBarOptions Options => new ProgressBarOptions
    {
        ForegroundColor = ConsoleColor.Yellow,
        ForegroundColorDone = ConsoleColor.Green,
        ProgressCharacter = '-'
    };
}
