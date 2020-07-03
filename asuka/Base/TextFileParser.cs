using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using Sharprompt;
using ShellProgressBar;
using asuka.API;
using asuka.Model;
using asuka.Utils;

namespace asuka.Base
{
  public static class TextFileParser
  {
    /// <summary>
    /// Reads the file and filters the valid codes and downloads them.
    /// </summary>
    /// <param name="filePath">Path to file to read</param>
    /// <param name="outPath">Path to save the downloaded doujinshi.</param>
    public static void ReadFile (string filePath, string outPath)
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
        using (var bar = new ProgressBar(ValidCodes.Length, "Downloading Tasks", GlobalOptions.ParentBar))
        {
          Parallel.ForEach(ValidCodes, new ParallelOptions { MaxDegreeOfParallelism = 2 }, task =>
          {
            // Retrieve the metadata first.
            Response data = Fetcher.SingleDoujin(task);
            DownloadBase.Download(data, outPath, bar);
          });
        }
      } else
      {
        Console.WriteLine("Then there's nothing to do.");
      }
    }
  }
}
