using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using CommandLine;
using Sharprompt;
using asuka.API;
using asuka.Base;
using asuka.Model;
using asuka.Exceptions;

namespace asuka
{
  class Program
  {
    #region Commandline Verbs and Options

    [Verb("get", HelpText = "Download or view a information of doujinshi.")]
    internal class GetDoujinshi
    {
      [Option('i', "input", Required = true, HelpText = "Input URL or filepath to a textfile.")]
      public string Input { get; set; }

      [Option('r', "readOnly", Required = false, HelpText = "View information only.")]
      public bool ReadOnly { get; set; }

      [Option('p', "pack", Required = false, HelpText = "Pack doujinshi as cbz archive.")]
      public bool Pack { get; set; }

      [Option('o', "output", Required = false, HelpText = "Output path. Default is the current working directory.")]
      public string Output { get; set; }
    }

    [Verb("search", HelpText = "Search a doujinshi")]
    internal class SearchDoujin
    {
      [Option('q', "query", Required = true, HelpText = "Search query")]
      public IEnumerable<string> SearchQuery { get; set; }

      [Option('p', "page", Required = true, HelpText = "Page number")]
      public int PageNumber { get; set; }

      [Option('o', "output", Required = false, HelpText = "Output path. Default is the current working directory.")]
      public string Output { get; set; }
    }

    [Verb("recommend", HelpText = "Recommend a doujinshi")]
    internal class RecommendDoujin
    {
      [Option('i', "input", Required = true, HelpText = "Input URL for recommendation.")]
      public string Input { get; set; }

      [Option('o', "output", Required = false, HelpText = "Output path. Default is current working directory.")]
      public string Output { get; set; }
    }

    [Verb("random", HelpText = "Pick a random doujinshi")]
    internal class RandomDoujin
    {
      [Option('o', "output", Required = false, HelpText = "Output path. Default is current working directory.")]
      public string Output { get; set; }
    }
    #endregion

    static void Main (string[] args)
    {
      Parser.Default.ParseArguments<GetDoujinshi, SearchDoujin, RecommendDoujin, RandomDoujin>(args)
        .WithParsed<GetDoujinshi>(GetDoujinshiParser)
        .WithParsed<SearchDoujin>(SearchDoujinshiParser)
        .WithParsed<RecommendDoujin>(RecommendDoujinshiParser)
        .WithParsed<RandomDoujin>(RandomDoujinshiParser)
        .WithNotParsed(HandleParseError);
    }

    #region Commandline Parsers
    private static void GetDoujinshiParser (GetDoujinshi opts)
    {
      if (!string.IsNullOrEmpty(opts.Input))
      {
        // Detect if the input string provided is a file path or a URL.
        // https://stackoverflow.com/a/56128519
        string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
        Regex regexp = new Regex(Pattern, RegexOptions.IgnoreCase);

        if (regexp.IsMatch(opts.Input))
        {
          // Now make sure the input is really a nhentai URL.
          string nhentaiPattern = @"^https?:\/\/(nhentai\.net)\b([//g]*)\b([\d]{1,6})\/?$";
          Regex nhRegexp = new Regex(nhentaiPattern, RegexOptions.IgnoreCase);

          if (nhRegexp.IsMatch(opts.Input))
          {
            Response doujinData = Fetcher.SingleDoujin(opts.Input);

            if (opts.ReadOnly)
            {
              DisplayDoujinMetadata.Display(doujinData);
            } else
            {
              DownloadBase.Download(doujinData, opts.Output);
            }
          } else
          {
            throw new InvalidArgumentException();
          }
        }
        else
        {
          TextFileParser.ReadFile(opts.Input, opts.Output);
        }
      } else
      {
        throw new InvalidArgumentException();
      }
    }

    private static void SearchDoujinshiParser (SearchDoujin opts)
    {
      if (!Utils.Validators.IsNullOrEmpty<string>(opts.SearchQuery))
      {
        SearchResponse data = Fetcher.SearchDoujin(opts.SearchQuery.ToList(), opts.PageNumber);

        Console.WriteLine("Displaying " + data.ItemsPerPage + " items");
        Console.WriteLine("Viewing page " + opts.PageNumber + " out of " + data.TotalPages);
        Console.WriteLine("");

        MultipleSelectionBase.PickResults(data.Result, data.ItemsPerPage, opts.Output);
      } else
      {
        throw new InvalidSearchQueryException();
      }
    }

    private static void RecommendDoujinshiParser (RecommendDoujin opts)
    {
      if (!string.IsNullOrEmpty(opts.Input))
      {
        RecommendationsResponse data = Fetcher.GetRecommendations(opts.Input);
        MultipleSelectionBase.PickResults(data.Result, 5, opts.Output);
      }
    }

    private static void RandomDoujinshiParser (RandomDoujin opts)
    {
      Response data = Fetcher.Random();
      DisplayDoujinMetadata.Display(data);

      var confirm = Prompt.Confirm("Are you sure to download this?");
      if (confirm)
      {
        DownloadBase.Download(data, opts.Output);
      } else
      {
        Console.WriteLine("Then there's nothing to do.");
      }
    }
    #endregion

    static void HandleParseError(IEnumerable<Error> errors)
    {

    }
  }
}
