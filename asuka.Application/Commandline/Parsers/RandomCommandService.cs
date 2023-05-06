using System;
using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Output;
using asuka.Application.Output.Writer;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Core.Output.Progress;
using asuka.Core.Requests;
using Sharprompt;

namespace asuka.Application.Commandline.Parsers;

public class RandomCommandService : ICommandLineParser
{
    private readonly IDownloader _download;
    private readonly IGalleryRequestService _api;
    private readonly IConsoleWriter _console;
    private readonly IProgressService _progress;
    private readonly ISeriesFactory _series;

    public RandomCommandService(
        IDownloader download,
        IGalleryRequestService api,
        IConsoleWriter console,
        IProgressService progress,
        ISeriesFactory series)
    {
        _download = download;
        _api = api;
        _console = console;
        _progress = progress;
        _series = series;
    }

    public async Task RunAsync(object options)
    {
        var opts = (RandomOptions)options;
        var totalNumbers = await _api.GetTotalGalleryCount();

        while (true)
        {
            var randomCode = new Random().Next(1, totalNumbers);
            var response = await _api.FetchSingle(randomCode.ToString());

            _console.WriteLine(response.ToReadable());

            var prompt = Prompt.Confirm("Are you sure to download this one?", true);
            if (!prompt)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                continue;
            }

            _series.AddChapter(response, opts.Output, 1);
            
            _progress.CreateMasterProgress(response.TotalPages, $"starting random id: {response.Id}");
            var progress = _progress.GetMasterProgress();

            _download.HandleOnProgress((_, e) =>
            {
                progress.Tick($"{e.Message} random id: {response.Id}");
            });

            await _download.Start(_series.GetSeries().Chapters.First());
            await _series.Close(opts.Pack ? progress : null);
            
            break;
        }
    }
}
