using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using asukav2.Lib;
using asukav2.Models;
using Sharprompt;

namespace asukav2
{
  public static class CommandLineParser
  {
    /// <summary>
    /// Parses the Get Command
    /// </summary>
    /// <param name="opts">Command Line options</param>
    /// <param name="cacheManager">Cache Manager Instance</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    public static async Task GetParserAsync(Get opts, CacheManagerLibrary cacheManager, CancellationToken token)
    {
      // https://stackoverflow.com/a/56128519
      const string pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
      var regexp = new Regex(pattern, RegexOptions.IgnoreCase);

      if (regexp.IsMatch(opts.Input))
      {
        var data = await ApiRequestLibrary.FetchSingleAsync(opts.Input, cacheManager, token);
        var info = await DisplayOutputLibrary.StringifyResponseAsync(data);

        Console.WriteLine(info);

        if (!opts.ReadOnly)
        {
          var download = new DownloadManager(data, opts.Output);
          await download.DownloadAsync(opts.Pack, null, cacheManager, token);
        }
      }
      else
      {
        await MultipleUrlProcessor.ParseTextFileAsync(opts.Input, opts.Output, opts.Pack, cacheManager, token);
      }
    }

    /// <summary>
    /// Parses the Search command
    /// </summary>
    /// <param name="opts"></param>
    /// <param name="cacheManager">Cache Manager Instance</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    public static async Task SearchParserAsync(Search opts, CacheManagerLibrary cacheManager, CancellationToken token)
    {
      var request = await ApiRequestLibrary.SearchDoujinAsync(opts, token);

      // Dump all results to cache.
      await cacheManager.WriteAllResultToCacheAsync(request.Result, token);

      Console.WriteLine($"Viewing {request.ItemsPerPage} on Page {opts.PageNumber} out of {request.TotalPages}");

      await SelectionPromptAsync(request.Result, (int) request.ItemsPerPage, opts.Output, opts.Pack, 
        cacheManager, token);
    }

    /// <summary>
    /// Parses the Recommend commad
    /// </summary>
    /// <param name="opts"></param>
    /// <param name="cacheManager">Cache Manager Instance</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    public static async Task RecommendParserAsync(Recommend opts, CacheManagerLibrary cacheManager, CancellationToken token)
    {
      var recommendedList = await ApiRequestLibrary.RecommendAsync(opts.Input, token);

      // Dump all results to cache.
      await cacheManager.WriteAllResultToCacheAsync(recommendedList.Result, token);

      await SelectionPromptAsync(recommendedList.Result, recommendedList.Result.Count, opts.Output, opts.Pack,
        cacheManager, token);
    }

    /// <summary>
    /// Parses the Random command
    /// </summary>
    /// <param name="opts"></param>
    /// <param name="cacheManager">Cache Manager Instance</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    public static async Task RandomParserAsync(Random opts, CacheManagerLibrary cacheManager, CancellationToken token)
    {
      var randomDoujin = await ApiRequestLibrary.RandomAsync(cacheManager, token);
      var info = await DisplayOutputLibrary.StringifyResponseAsync(randomDoujin);

      // Add to the database.
      await cacheManager.AddDataToCacheAsync(randomDoujin.Id.ToString(), randomDoujin, token);

      Console.WriteLine(info);

      var confirm = Prompt.Confirm("Are you sure to download this?", true);
      if (confirm)
      {
        var downloader = new DownloadManager(randomDoujin, opts.Output);
        await downloader.DownloadAsync(opts.Pack, null, cacheManager, token);
        return;
      }

      Console.WriteLine("We got nothing to do. Exiting.");
    }

    private static async Task SelectionPromptAsync(IReadOnlyCollection<ResponseModel> result,
      int itemsPerPage, string outputPath, bool pack, CacheManagerLibrary cache, CancellationToken token)
    {
      // No result.
      if (result.Count <= 0)
      {
        Console.WriteLine("No results found. Exiting.");
        return;
      }

      var selection = Prompt.MultiSelect("Select to download", result, itemsPerPage,
        valueSelector: (resp) =>
        {
          var title = string.IsNullOrEmpty(resp.Title.Japanese)
            ? (string.IsNullOrEmpty(resp.Title.English) ? resp.Title.Pretty : resp.Title.English)
            : resp.Title.Japanese;
          return title;
        });

      await MultipleUrlProcessor.ParseResponseAsync(selection.ToList(), outputPath, pack, cache, token);
    }
  }
}
