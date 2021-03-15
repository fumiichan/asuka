using System;
using ShellProgressBar;

namespace asukav2.Config
{
  public static class GlobalProgressConfig
  {
    public static readonly ProgressBarOptions BarOptions = new()
    {
      ForegroundColor = ConsoleColor.Yellow,
      ForegroundColorDone = ConsoleColor.Green,
      ProgressCharacter = '#'
    };

    public static readonly ProgressBarOptions CompressBarOption = new()
    {
      ForegroundColor = ConsoleColor.DarkCyan,
      ForegroundColorDone = ConsoleColor.Cyan,
      ProgressCharacter = '/'
    };
  }
}
