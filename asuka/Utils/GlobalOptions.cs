using System;
using ShellProgressBar;

namespace asuka.Utils
{
  public static class GlobalOptions
  {
    public readonly static ProgressBarOptions ParentBar = new ProgressBarOptions
    {
      ForegroundColor = ConsoleColor.Yellow,
      ForegroundColorDone = ConsoleColor.Green,
      ProgressCharacter = '─'
    };

    public readonly static ProgressBarOptions ChildBar = new ProgressBarOptions
    {
      ForegroundColor = ConsoleColor.Cyan,
      ForegroundColorDone = ConsoleColor.Green,
      ProgressCharacter = '─',
      CollapseWhenFinished = true
    };
  }
}
