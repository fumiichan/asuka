using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RestSharp;
using Newtonsoft.Json;
using asuka.Internal.Cache;
using asuka.Model;
using asuka.Exceptions;

namespace asuka.API
{
  class Fetcher
  {
    private static RestClient client = new RestClient("https://nhentai.net/api");

    /// <summary>
    /// Fetches a nhentai information
    /// </summary>
    /// <param name="url">A nhentai doujin code.</param>
    /// <returns>Doujin metadata</returns>
    public static Response SingleDoujin (string url)
    {
      // Get the 1-6 digit codes from the URL.
      string code = Regex.Match(url, @"\d+").Value;

      if (string.IsNullOrEmpty(code))
      {
        throw new InvalidArgumentException();
      }

      RestRequest request = new RestRequest("/gallery/" + code, DataFormat.Json);
      CacheManager cache = new CacheManager(int.Parse(code));

      if (cache.ReadCache() == null)
      {
        IRestResponse response = client.Get(request);
        if (response.IsSuccessful)
        {
          var result = JsonConvert.DeserializeObject<Response>(response.Content);
          cache.WriteCache(result);
          return result;
        }
        else
        {
          throw new APIRequestFailedException();
        }
      } else
      {
        return cache.ReadCache();
      }
    }

    /// <summary>
    /// Search on nhentai
    /// </summary>
    /// <param name="queries">Search queries</param>
    /// <param name="page">Page number</param>
    /// <returns>Search Query Response</returns>
    public static SearchResponse SearchDoujin (List<string> queries, int page)
    {
      if (page <= 0) page = 1;
      if (!queries.Any()) throw new InvalidArgumentException();

      RestRequest request = new RestRequest("/galleries/search", DataFormat.Json);
      request.AddParameter("query", string.Join(" ", queries));
      request.AddParameter("page", page.ToString());

      IRestResponse response = client.Get(request);
      if (response.IsSuccessful)
      {
        var result = JsonConvert.DeserializeObject<SearchResponse>(response.Content);
        return result;
      } else
      {
        throw new APIRequestFailedException();
      }
    }

    /// <summary>
    /// Gets Recommended Doujins
    /// </summary>
    /// <param name="url">URL of the doujin you want to get recommendations</param>
    /// <returns>Recommendations</returns>
    public static RecommendationsResponse GetRecommendations (string url)
    {
      string code = Regex.Match(url, @"\d+").Value;

      if (string.IsNullOrEmpty(code))
      {
        throw new InvalidArgumentException();
      }

      RestRequest request = new RestRequest("/gallery/" + code + "/related", DataFormat.Json);

      IRestResponse response = client.Get(request);
      if (response.IsSuccessful)
      {
        var result = JsonConvert.DeserializeObject<RecommendationsResponse>(response.Content);
        return result;
      } else
      {
        throw new APIRequestFailedException();
      }
    }

    /// <summary>
    /// Random Doujin
    /// </summary>
    /// <returns>Random Doujin Data.</returns>
    public static Response Random ()
    {
      // First we will get the home page data.
      RestRequest request = new RestRequest("/galleries/all", DataFormat.Json);
      request.AddParameter("page", "1");

      var response = client.Get(request);
      if (response.IsSuccessful)
      {
        var result = JsonConvert.DeserializeObject<SearchResponse>(response.Content);

        // Generate a random ID for the doujin to select.
        Random rand = new Random();
        int Id = rand.Next(1, result.Result[0].Id);

        Response data = SingleDoujin("https://nhentai.net/g/" + Id.ToString());
        return data;
      } else
      {
        throw new APIRequestFailedException();
      }
    }
  }
}
