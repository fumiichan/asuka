using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using asukav2.Models;
using Newtonsoft.Json;
using RestSharp;

namespace asukav2.Lib
{
  public static class ApiRequestLibrary
  {
    /// <summary>
    ///   Fetches single nhentai doujin link information
    /// </summary>
    /// <param name="url">A nhentai doujin code.</param>
    /// <param name="cache">Cache Manager Instance</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public static async Task<ResponseModel> FetchSingleAsync(string url, CacheManagerLibrary cache,
      CancellationToken token)
    {
      if (token.IsCancellationRequested)
      {
        token.ThrowIfCancellationRequested();
      }

      // Test if the URL is a valid nhentai URL.
      const string pattern = @"^http(s)?:\/\/(nhentai\.net)\b([//g]*)\b([\d]{1,6})\/?$";
      var regexp = new Regex(pattern, RegexOptions.IgnoreCase);

      // Get the 1-6 digit codes from the URL.
      var code = Regex.Match(url, @"\d+").Value;

      if (!regexp.IsMatch(url) || string.IsNullOrEmpty(code))
      {
        throw new ArgumentException("Invalid URL.");
      }

      // Fetch the information from the database if present.
      var search = await cache.GetDoujinInformationAsync(code, token);
      if (search != null)
      {
        return search;
      }

      var client = new RestClient("https://nhentai.net/api");
      var request = new RestRequest($"/gallery/{code}", DataFormat.Json);
      var response = await client.ExecuteAsync(request, token);

      if (!response.IsSuccessful)
      {
        throw new HttpRequestException("Failed to fetch information.");
      }

      var result = JsonConvert.DeserializeObject<ResponseModel>(response.Content);
      await cache.AddDataToCacheAsync(code, result, token);

      return result;
    }

    /// <summary>
    ///   Search for doujins on nhentai
    /// </summary>
    /// <param name="args">Search arguments</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    public static async Task<SearchResponseModel> SearchDoujinAsync(Search args, CancellationToken token)
    {
      if (token.IsCancellationRequested)
      {
        token.ThrowIfCancellationRequested();
      }

      // Set page to 1 if page specified is 0.
      var page = args.PageNumber;
      if (args.PageNumber <= 0)
      {
        page = 1;
      }

      // Create list of queries
      var finalQueries = new List<string>();

      var client = new RestClient("https://nhentai.net/api");
      var request = new RestRequest("/galleries/search", DataFormat.Json);

      finalQueries.AddRange(args.Query);
      finalQueries.AddRange(args.Exclude.Select(v => $"-{v}"));

      // Ensure we don't collide with options such as --pageMin or --pageMax with --pageCount.
      // The same with --dateRangeMin or --dateRangeMax with --dateUploaded.
      if ((args.PageRangeMinimum != null || args.PageRangeMaximum != null) && args.PageSpecific != null)
      {
        throw new ArgumentException("You cannot combine --pageMin or --pageMax with --pageCount.");
      }

      if ((!string.IsNullOrEmpty(args.DateRangeMin) || !string.IsNullOrEmpty(args.DateRangeMax)) && !string.IsNullOrEmpty(args.DateUploaded))
      {
        throw new ArgumentException("You cannot combine --dateRangeMin or --dateRangeMax with --dateUploaded.");
      }

      // Start of Cringey if statements
      if (args.PageRangeMinimum != null)
      {
        finalQueries.Add($"pages:>={args.PageRangeMinimum}");
      }
      if (args.PageRangeMaximum != null)
      {
        finalQueries.Add($"pages:<={args.PageRangeMaximum}");
      }
      if (args.PageSpecific != null)
      {
        finalQueries.Add($"pages:{args.PageSpecific}");
      }

      if (!string.IsNullOrEmpty(args.DateRangeMin))
      {
        finalQueries.Add($"uploaded:>={args.DateRangeMin}");
      }
      if (!string.IsNullOrEmpty(args.DateRangeMax))
      {
        finalQueries.Add($"uploaded:<={args.DateRangeMax}");
      }
      if (!string.IsNullOrEmpty(args.DateUploaded))
      {
        finalQueries.Add($"uploaded:{args.DateUploaded}");
      }

      request.AddParameter("query", string.Join(" ", finalQueries));
      request.AddParameter("page", page.ToString());

      switch (args.Sort)
      {
        case SortOptions.Popular:
          request.AddParameter("sort", "popular");
          break;
        case SortOptions.PopularToday:
          request.AddParameter("sort", "popular-today");
          break;
        case SortOptions.PopularWeek:
          request.AddParameter("sort", "popular-week");
          break;
        case SortOptions.Recent:
          request.AddParameter("sort", "date");
          break;
        default:
          throw new NotImplementedException("Sort option is not implemented.");
      }

      var response = await client.ExecuteAsync(request, token);
      if (!response.IsSuccessful)
      {
        throw new HttpRequestException("Failed to fetch search results.");
      }

      var result = JsonConvert.DeserializeObject<SearchResponseModel>(response.Content);
      return result;
    }

    /// <summary>
    ///   Fetch Recommendations
    /// </summary>
    /// <param name="url">Doujin URL to get for recommendations</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    public static async Task<ListResponseModel> RecommendAsync(string url, CancellationToken token)
    {
      if (token.IsCancellationRequested)
      {
        token.ThrowIfCancellationRequested();
      }

      // Test if the URL is a valid nhentai URL.
      const string pattern = @"^http(s)?:\/\/(nhentai\.net)\b([//g]*)\b([\d]{1,6})\/?$";
      var regexp = new Regex(pattern, RegexOptions.IgnoreCase);

      var code = Regex.Match(url, @"\d+").Value;

      if (!regexp.IsMatch(url) || string.IsNullOrEmpty(code))
      {
        throw new ArgumentException("Invalid URL.");
      }

      var client = new RestClient("https://nhentai.net/api");
      var request = new RestRequest($"/gallery/{code}/related", DataFormat.Json);
      var response = await client.ExecuteAsync(request, token);

      if (!response.IsSuccessful)
      {
        throw new HttpRequestException("Failed to fetch recommendations");
      }

      var result = JsonConvert.DeserializeObject<ListResponseModel>(response.Content);
      return result;
    }

    /// <summary>
    ///   Randomise Doujin Selection
    /// </summary>
    /// <param name="cache">Cache Manager Instance</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    public static async Task<ResponseModel> RandomAsync(CacheManagerLibrary cache, CancellationToken token)
    {
      if (token.IsCancellationRequested)
      {
        token.ThrowIfCancellationRequested();
      }

      var initialCount = 300000;

      var cachedCount = await cache.GetDoujinCountCacheAsync(DateTime.Now, token);
      if (cachedCount != null)
      {
        initialCount = cachedCount.Count;
      }
      else
      {
        var client = new RestClient("https://nhentai.net/api");
        var request = new RestRequest("/galleries/all", DataFormat.Json);
        var requestResponse = await client.ExecuteAsync(request, token);

        if (requestResponse.IsSuccessful)
        {
          var result = JsonConvert.DeserializeObject<SearchResponseModel>(requestResponse.Content);
          initialCount = result.Result[0].Id;

          // Store the information to the cache so that it will be used on further random commands
          // for 24 hours.
          await cache.AddDoujinCountCacheAsync(DateTime.Now, initialCount, token);
        }
        else
        {
          Console.WriteLine("Warning: Failed to fetch information. Using predefined number instead.");
        }
      }

      // Random the ID.
      var random = new System.Random();
      var id = random.Next(1, initialCount);

      var response = await FetchSingleAsync($"https://nhentai.net/g/{id}", cache, token);
      return response;
    }
  }
}