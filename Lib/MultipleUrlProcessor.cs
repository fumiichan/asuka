using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Sharprompt;
using ShellProgressBar;
using asukav2.Config;
using asukav2.Models;

namespace asukav2.Lib
{
  public static class MultipleUrlProcessor
  {
    /// <summary>
    /// Downloads list of URLs.
    /// </summary>
    /// <param name="urls">List of URLs to download</param>
    /// <param name="outputPath">Path to store the files</param>
    /// <param name="pack">Use compression</param>
    /// <param name="cache">Cache Manager Instance</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    private static async Task ParseListAsync(IReadOnlyCollection<string> urls, string outputPath, bool pack, 
      CacheManagerLibrary cache, CancellationToken token)
    {
      if (token.IsCancellationRequested)
      {
        token.ThrowIfCancellationRequested();
      }

      // Progress bar.
      using var progress = new ProgressBar(urls.Count, "Downloading Collection", 
        GlobalProgressConfig.BarOptions);

      foreach (var url in urls)
      {
        var info = await ApiRequestLibrary.FetchSingleAsync(url, cache, token);
        var downloader = new DownloadManager(info, outputPath);

        await downloader.DownloadAsync(pack, progress, cache, token);
        progress.Tick();
      }
    }

    /// <summary>
    /// Downloads the list of Responses.
    /// </summary>
    /// <param name="responses">List of nhentai API's responses</param>
    /// <param name="outputPath">Path to store the files</param>
    /// <param name="pack">Use compression</param>
    /// <param name="cache">Cache Manager Instance</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    public static async Task ParseResponseAsync(IReadOnlyCollection<ResponseModel> responses, string outputPath,
      bool pack, CacheManagerLibrary cache, CancellationToken token)
    {
      if (token.IsCancellationRequested)
      {
        token.ThrowIfCancellationRequested();
      }

      // Progress bar.
      using var progress = new ProgressBar(responses.Count, "Downloading Collection", 
        GlobalProgressConfig.BarOptions);

      foreach (var downloader in responses.Select(response => new DownloadManager(response, outputPath)))
      {
        await downloader.DownloadAsync(pack, progress, cache, token);
        progress.Tick();
      }
    }

    /// <summary>
    /// Parses Text File Contents
    /// </summary>
    /// <param name="textPath">Text File to read and parse it's contents</param>
    /// <param name="outputPath">Path to store the files</param>
    /// <param name="pack">Use compression</param>
    /// <param name="cache">Cache Manager</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    public static async Task ParseTextFileAsync(string textPath, string outputPath, bool pack, 
      CacheManagerLibrary cache, CancellationToken token)
    {
      if (token.IsCancellationRequested)
      {
        token.ThrowIfCancellationRequested();
      }

      if (!File.Exists(textPath))
      {
        throw new FileNotFoundException($"The file '{textPath}' could not be found.");
      }

      const string nhentaiUrlRegex = @"^http(s)?:\/\/(nhentai\.net)\b([//g]*)\b([\d]{1,6})\/?$";
      var regex = new Regex(nhentaiUrlRegex, RegexOptions.IgnoreCase);

      var textContent = await File.ReadAllLinesAsync(textPath, token);

      // Ensure that we will collect only the matched URLs on the text.
      var urls = textContent.Where((line) => regex.IsMatch(line)).ToList();

      var confirm = Prompt.Confirm($"Found {urls.Count} to download. Do you want to continue?", true);
      if (confirm)
      {
        await ParseListAsync(urls, outputPath, pack, cache, token);
        return;
      }

      Console.WriteLine("Then we got nothing to do.");
    }
  }
}