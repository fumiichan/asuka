using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Utilities;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Core.Extensions;
using asuka.Core.Output.Progress;
using asuka.Core.Requests;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

public class GetCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IValidator<GetOptions> _validator;
    private readonly IDownloader _download;
    private readonly IProgressService _progress;
    private readonly ISeriesFactory _series;
    private readonly ILogger _logger;

    public GetCommandService(
        IGalleryRequestService api,
        IValidator<GetOptions> validator,
        IDownloader download,
        IProgressService progress,
        ISeriesFactory series,
        ILogger logger)
    {
        _api = api;
        _validator = validator;
        _download = download;
        _progress = progress;
        _series = series;
        _logger = logger;
    }

    private async Task DownloadTask(int input, bool pack, bool readOnly, string outputPath)
    {
        var response = await _api.FetchSingle(input.ToString());
        _logger.LogInformation(response.BuildReadableInformation());

        // Don't download.
        if (readOnly)
        {
            return;
        }
        
        _series.AddChapter(response, outputPath);

        _progress.CreateMasterProgress(response.TotalPages, $"downloading: {response.Id}");
        var progress = _progress.GetMasterProgress();

        _download.HandleOnProgress((_, e) =>
        {
            progress.Tick($"{e.Message}: {response.Id}");
        });

        await _download.Start(_series.GetSeries().Chapters.First());
        await _series.Close(pack ? progress : null);
    }

    public async Task Run(object options)
    {
        var opts = (GetOptions)options;
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintErrors(_logger);
            return;
        }

        foreach (var code in opts.Input)
        {
            await DownloadTask(code, opts.Pack, opts.ReadOnly, opts.Output);
        }
    }
}
