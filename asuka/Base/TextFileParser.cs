using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sharprompt;
using ShellProgressBar;
using asuka.API;
using asuka.Model;
using asuka.Utils;
using asuka.Internal;

namespace asuka.Base
{
  public static class TextFileParser
  {
    /// <summary>
    /// Reads the file and filters the valid codes and downloads them.
    /// </summary>
    /// <param name="filePath">Path to file to read</param>
    /// <param name="outPath">Path to save the downloaded doujinshi.</param>
    public static void ReadFile (string filePath, string outPath, bool pack)
    {
      if (!File.Exists(filePath))
      {
        throw new FileNotFoundException("Text file you specified cannot be found.");
      }

      string nhentaiPattern = @"^https?:\/\/(nhentai\.net)\b([//g]*)\b([\d]{1,6})\/?$";
      Regex nhRegexp = new Regex(nhentaiPattern, RegexOptions.IgnoreCase);

      string[] TextContent = File.ReadAllLines(filePath);
      string[] ValidCodes = TextContent.Where(v =>
      {
        return nhRegexp.IsMatch(v);
      }).ToArray();

      if (ValidCodes.Length <= 0)
      {
        Console.WriteLine("No valid codes found.");
        return;
      }

      Console.WriteLine($"Found total of {ValidCodes.Length} valid codes.");

      var confirm = Prompt.Confirm("Are you sure to download them?");

      if (confirm)
      {
        using var bar = new ProgressBar(ValidCodes.Length, "Downloading Tasks", GlobalOptions.ParentBar);

        Configuration config = new Configuration();
        int maxParallelLimit = int.Parse(config.GetConfigurationValue("parallelTasks"));

        using SemaphoreSlim concurrency = new SemaphoreSlim(maxParallelLimit);

        List<Task> tasks = ValidCodes.Select(value =>
        {
          concurrency.Wait();

          return Task.Factory.StartNew(() =>
          {
            try
            {
              Response data = Fetcher.SingleDoujin(value);
              DownloadBase download = new DownloadBase(data, outPath);
              download.Download(pack, bar);

              bar.Tick();
            }
            finally
            {
              concurrency.Release();
            }
          });
        }).ToList();

        Task.WaitAll(tasks.ToArray());
      } else
      {
        Console.WriteLine("Then there's nothing to do.");
      }
    }
  }
}
