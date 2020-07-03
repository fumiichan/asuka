using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ShellProgressBar;
using Sharprompt;
using asuka.Model;
using asuka.Utils;

namespace asuka.Base
{
  class MultipleSelectionBase
  {
    /// <summary>
    /// Displays a multiple selection on the console and lets the user what to do next.
    /// </summary>
    /// <param name="Result">nhentai search result</param>
    /// <param name="PageSize">total number of results returned</param>
    /// <param name="output">path to save the downloaded doujinshi</param>
    public static void PickResults (List<Response> Result, int PageSize, string output, bool readOnly = false)
    {
      Func<Response, string> selector = delegate (Response resp)
      {
        return resp.Title.English;
      };
      IEnumerable<Response> selection = Prompt.MultiSelect("Pick one or more in the results", Result, pageSize: PageSize, valueSelector: selector);
      List<Response> options = selection.ToList();

      // Do not download when readOnly is true.
      if (readOnly)
      {
        foreach (Response resp in options)
        {
          DisplayDoujinMetadata.Display(resp);
        }

        return;
      }

      var action = Prompt.Confirm("Are you sure to download these you picked?");
      if (action)
      {
        using (var bar = new ProgressBar(options.Count(), "Downloading Results", GlobalOptions.ParentBar))
        {
          Parallel.ForEach(options, new ParallelOptions { MaxDegreeOfParallelism = 2 }, task =>
          {
            DownloadBase.Download(task, output, bar);
            bar.Tick();
          });
        }
      } else
      {
        Console.WriteLine("Then there's nothing to do.");
      }
    }
  }
}
