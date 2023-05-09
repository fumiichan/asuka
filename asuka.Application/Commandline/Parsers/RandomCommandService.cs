using System;
using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Core.Extensions;
using asuka.Core.Output.Progress;
using asuka.Core.Requests;
using Microsoft.Extensions.Logging;
using Sharprompt;

namespace asuka.Application.Commandline.Parsers;

public class RandomCommandService : ICommandLineParser
{
    private readonly IDownloader _download;
    private readonly IGalleryRequestService _api;
    private readonly IProgressService _progress;
    private readonly ISeriesFactory _series;
    private readonly ILogger _logger;

    public RandomCommandService(
        IDownloader download,
        IGalleryRequestService api,
        IProgressService progress,
        ISeriesFactory series,
        ILogger logger)
    {
        _download = download;
        _api = api;
        _progress = progress;
        _series = series;
        _logger = logger;
    }

    public async Task Run(object options)
    {
        var opts = (RandomOptions)options;
        var totalNumbers = await _api.GetTotalGalleryCount();

        while (true)
        {
            var randomCode = new Random().Next(1, totalNumbers);
            var response = await _api.FetchSingle(randomCode.ToString());

            _logger.LogInformation(response.BuildReadableInformation());

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
