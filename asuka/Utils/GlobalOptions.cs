using System;
using ShellProgressBar;

namespace asuka.Utils
{
  class GlobalOptions
  {
    public static ProgressBarOptions ParentBar = new ProgressBarOptions
    {
      ForegroundColor = ConsoleColor.Yellow,
      ForegroundColorDone = ConsoleColor.Green,
      ProgressCharacter = '─'
    };

    public static ProgressBarOptions ChildBar = new ProgressBarOptions
    {
      ForegroundColor = ConsoleColor.Cyan,
      ForegroundColorDone = ConsoleColor.Green,
      ProgressCharacter = '─'
    };
  }
}
