using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    /// <returns></returns>
    public static async Task GetParserAsync(Get opts, CacheManagerLibrary cacheManager)
    {
      // https://stackoverflow.com/a/56128519
      const string pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
      var regexp = new Regex(pattern, RegexOptions.IgnoreCase);

      if (regexp.IsMatch(opts.Input))
      {
        var data = await ApiRequestLibrary.FetchSingleAsync(opts.Input, cacheManager);
        var info = await DisplayOutputLibrary.StringifyResponseAsync(data);

        Console.WriteLine(info);

        if (!opts.ReadOnly)
        {
          var download = new DownloadManager(data, opts.Output);
          await download.DownloadAsync(opts.Pack, null, cacheManager);
        }
      }
      else
      {
        await MultipleUrlProcessor.ParseTextFileAsync(opts.Input, opts.Output, opts.Pack, cacheManager);
      }
    }

    /// <summary>
    /// Parses the Search command
    /// </summary>
    /// <param name="opts"></param>
    /// <param name="cacheManager">Cache Manager Instance</param>
    /// <returns></returns>
    public static async Task SearchParserAsync(Search opts, CacheManagerLibrary cacheManager)
    {
      var request = await ApiRequestLibrary.SearchDoujinAsync(opts);

      // Dump all results to cache.
      await cacheManager.WriteAllResultToCacheAsync(request.Result);

      Console.WriteLine($"Viewing {request.ItemsPerPage} on Page {opts.PageNumber} out of {request.TotalPages}");

      await SelectionPromptAsync(request.Result, (int) request.ItemsPerPage, opts.Output, opts.Pack, 
        cacheManager);
    }

    /// <summary>
    /// Parses the Recommend commad
    /// </summary>
    /// <param name="opts"></param>
    /// <param name="cacheManager">Cache Manager Instance</param>
    /// <returns></returns>
    public static async Task RecommendParserAsync(Recommend opts, CacheManagerLibrary cacheManager)
    {
      var recommendedList = await ApiRequestLibrary.RecommendAsync(opts.Input);

      // Dump all results to cache.
      await cacheManager.WriteAllResultToCacheAsync(recommendedList.Result);

      await SelectionPromptAsync(recommendedList.Result, recommendedList.Result.Count, opts.Output, opts.Pack,
        cacheManager);
    }

    /// <summary>
    /// Parses the Random command
    /// </summary>
    /// <param name="opts"></param>
    /// <param name="cacheManager">Cache Manager Instance</param>
    /// <returns></returns>
    public static async Task RandomParserAsync(Random opts, CacheManagerLibrary cacheManager)
    {
      var randomDoujin = await ApiRequestLibrary.RandomAsync(cacheManager);
      var info = await DisplayOutputLibrary.StringifyResponseAsync(randomDoujin);

      // Add to the database.
      await cacheManager.AddDataToCacheAsync(randomDoujin.Id.ToString(), randomDoujin);

      Console.WriteLine(info);

      var confirm = Prompt.Confirm("Are you sure to download this?", true);
      if (confirm)
      {
        var downloader = new DownloadManager(randomDoujin, opts.Output);
        await downloader.DownloadAsync(opts.Pack, null, cacheManager);
        return;
      }

      Console.WriteLine("We got nothing to do. Exiting.");
    }

    /// <summary>
    /// Processes Search Result
    /// </summary>
    /// <param name="result"></param>
    /// <param name="itemsPerPage"></param>
    /// <param name="outputPath"></param>
    /// <param name="pack"></param>
    /// <param name="cache"></param>
    /// <returns></returns>
    private static async Task SelectionPromptAsync(IReadOnlyCollection<ResponseModel> result,
      int itemsPerPage, string outputPath, bool pack, CacheManagerLibrary cache)
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

      await MultipleUrlProcessor.ParseResponseAsync(selection.ToList(), outputPath, pack, cache);
    }
  }
}
