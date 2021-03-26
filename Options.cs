using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global
namespace asukav2
{
  internal interface IRequiresInputOption
  {
    [Option('i', "input",
      Required = true,
      HelpText = "URL or Path to text file.")]
    string Input { get; set; }
  }

  internal interface ICommonOptions
  {
    [Option('p', "pack",
      Default = false,
      Required = false,
      HelpText = "Compress your download to CBZ archive.")]
    bool Pack { get; set; }

    [Option('o', "output",
      Required = false,
      HelpText = "Destination path of your download. Defaults to current working directory.")]
    string Output { get; set; }
  }

  public enum SortOptions
  {
    PopularToday,
    PopularWeek,
    Popular,
    Recent
  }

  [Verb("get", HelpText = "Download or view a information about doujinshi.")]
  public class Get : IRequiresInputOption, ICommonOptions
  {
    public string Input { get; set; }
    public bool Pack { get; set; }

    [Option('r', "readonly",
      Required = false,
      Default = false,
      HelpText = "View the information only. Do not download.")]
    public bool ReadOnly { get; set; }

    public string Output { get; set; }

    [Usage]
    public static IEnumerable<Example> Examples
    {
      get
      {
        yield return new Example("download", new Get {Input = "https://nhentai.net/g/177013"});
        yield return new Example("view info only", new Get {Input = "https://nhentai.net/g/177013", ReadOnly = true});
        yield return new Example("download on different path",
          new Get {Input = "https://nhentai.net/g/177013", Output = @"C:\Windows\System32"});
        yield return new Example("compress to CBZ", new Get {Input = "https://nhentai.net/g/177013", Pack = true});
      }
    }
  }

  [Verb("search", HelpText = "Search a doujinshi.")]
  public class Search : ICommonOptions
  {
    [Option('q', "query",
      Required = false,
      HelpText = "Search Queries")]
    public IEnumerable<string> Query { get; set; }

    [Option('e', "exclude",
      Required = false,
      HelpText = "Specify Excluded tag")]
    public IEnumerable<string> Exclude { get; set; }

    [Option("page",
      Default = 1,
      Required = true,
      HelpText = "Page number")]
    public uint PageNumber { get; set; }

    [Option("pageRange",
      Required = false,
      HelpText = "Filter for specifying count of pages in gallery.")]
    public IEnumerable<string> PageRange { get; set; }

    [Option("dateRange",
      Required = false,
      HelpText = "Filter for specifying exact upload timeframe of the gallery.")]
    public IEnumerable<string> DateRange { get; set; }

    [Option('s',"sort",
      Required = false,
      Default = SortOptions.Recent,
      HelpText = "Sort results")]
    public SortOptions Sort { get; set; }

    public bool Pack { get; set; }
    public string Output { get; set; }

    [Usage]
    public static IEnumerable<Example> Examples
    {
      get
      {
        yield return new Example("normal usage", new Search {Query = new[] {"yaoi", "\"males only\""}, PageNumber = 1});
        yield return new Example("normal usage but exclude",
          new Search {Query = new[] {"netorare", "group"}, PageNumber = 1});
        yield return new Example("no queries but exact amount of pages only",
          new Search {PageRange = new[] {"20"}, PageNumber = 1});
        yield return new Example("no queries but between minimum and maximum pages",
          new Search {PageRange = new[] {">20", "<50"}, PageNumber = 1});
        yield return new Example("no queries but exact timeframe only",
          new Search {DateRange = new[] {"1d"}, PageNumber = 1});
        yield return new Example("no queries but range timeframes",
          new Search {DateRange = new[] {">2d", "<=5d"}, PageNumber = 1});
        yield return new Example("filter by popularity", new Search {Sort = SortOptions.Popular, PageNumber = 1});
      }
    }
  }

  [Verb("recommend", HelpText = "Recommend something related.")]
  public class Recommend : IRequiresInputOption, ICommonOptions
  {
    public string Input { get; set; }
    public bool Pack { get; set; }
    public string Output { get; set; }

    [Usage]
    public static IEnumerable<Example> Examples
    {
      get
      {
        yield return new Example("normal usage", new Recommend {Input = "https://nhentai.net/g/177013"});
      }
    }
  }

  [Verb("random", HelpText = "Pick something random.")]
  public class Random : ICommonOptions
  {
    public bool Pack { get; set; }
    public string Output { get; set; }
  }
}
