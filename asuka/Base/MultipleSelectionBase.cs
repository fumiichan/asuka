using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ShellProgressBar;
using Sharprompt;
using asuka.Model;
using asuka.Utils;
using asuka.Internal;

namespace asuka.Base
{
  public static class MultipleSelectionBase
  {
    /// <summary>
    /// Displays a multiple selection on the console and lets the user what to do next.
    /// </summary>
    /// <param name="Result">nhentai search result</param>
    /// <param name="PageSize">total number of results returned</param>
    /// <param name="output">path to save the downloaded doujinshi</param>
    public static void PickResults (List<Response> Result, int PageSize, string output, bool pack)
    {
      static string selector(Response resp)
      {
        return resp.Title.English;
      }
      IEnumerable<Response> selection = Prompt.MultiSelect("Pick one or more in the results", Result, pageSize: PageSize, valueSelector: selector);
      List<Response> options = selection.ToList();

      var action = Prompt.Confirm("Are you sure to download these you picked?");
      if (action)
      {
        using var bar = new ProgressBar(options.Count(), "Downloading Results", GlobalOptions.ParentBar);

        Configuration config = new Configuration();
        int maxParallelLimit = int.Parse(config.GetConfigurationValue("parallelTasks"));

        using SemaphoreSlim concurrency = new SemaphoreSlim(maxParallelLimit);

        List<Task> tasks = options.Select(value =>
        {
          concurrency.Wait();

          return Task.Factory.StartNew(() =>
          {
            try
            {
              DownloadBase download = new DownloadBase(value, output);
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
