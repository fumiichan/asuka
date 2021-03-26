using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using asukav2.Lib;
using CommandLine.Text;

namespace asukav2
{
  internal static class Program
  {
    /// <summary>
    /// Main entry point
    /// </summary>
    /// <param name="args">Program arguments</param>
    private static async Task Main(string[] args)
    {
      // Initialise the Cache Manager here.
      if (!CacheManagerLibrary.IsDatabaseExists())
      {
        await CacheManagerLibrary.AutoMigrateDatabaseAsync();
      }
      var cache = new CacheManagerLibrary();

      try
      {
        var result = new Parser(with => with.HelpWriter = null)
          .ParseArguments<Get, Search, Recommend, Random>(args);
        await result.MapResult(
          async (Get opts) => await CommandLineParser.GetParserAsync(opts, cache),
          async (Search opts) => await CommandLineParser.SearchParserAsync(opts, cache),
          async (Recommend opts) => await CommandLineParser.RecommendParserAsync(opts, cache),
          async (Random opts) => await CommandLineParser.RandomParserAsync(opts, cache),
          (err) =>
          {
            var helpText = HelpText.AutoBuild(result, h =>
            {
              h.AddEnumValuesToHelpText = true;
              return h;
            }, e => e);

            Console.WriteLine(helpText);

            return Task.FromResult(1);
          }
        );
      }
      catch (Exception e)
      {
        Console.WriteLine($"Oops an error occured. Error: {e.Message}");
      }
      finally
      {
        // Save database changes.
        await cache.SaveChangesAsync();
        cache.Dispose();
      }
    }
  }
}
