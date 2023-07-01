using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Utilities;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Core.Output.Progress;
using asuka.Core.Requests;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

public class FileCommandService : ICommandLineParser
{
    private readonly IEnumerable<IGalleryRequestService> _apis;
    private readonly IDownloader _download;
    private readonly IProgressService _progressService;
    private readonly ISeriesFactory _series;
    private readonly IValidator<FileCommandOptions> _validator;
    private readonly ILogger _logger;

    public FileCommandService(
        IEnumerable<IGalleryRequestService> apis,
        IDownloader download,
        IProgressService progressService,
        ISeriesFactory series,
        IValidator<FileCommandOptions> validator,
        ILogger logger)
    {
        _apis = apis;
        _download = download;
        _progressService = progressService;
        _series = series;
        _validator = validator;
        _logger = logger;
    }

    public async Task Run(object options)
    {
        var opts = (FileCommandOptions)options;
        
        var validationResult = await _validator.ValidateAsync(opts);

        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintErrors(_logger);
            return;
        }

        await ExecuteCommand(opts);
    }

    private async Task ExecuteCommand(FileCommandOptions opts)
    {
        var textFile = await File.ReadAllLinesAsync(opts.FilePath, Encoding.UTF8)
            .ConfigureAwait(false);

        _progressService.CreateMasterProgress(textFile.Length, "downloading from text file...");
        var progress = _progressService.GetMasterProgress();

        foreach (var url in textFile)
        {
            // Allows dynamic provider switching to allow multiple providers to be in single file
            var provider = _apis.GetFirstByHostname(url);
            if (provider is null)
            {
                var failProgress = _progressService.NestToMaster(1, $"skipped: {url}");
                failProgress.Tick();
                progress.Tick();
                continue;
            }

            var response = await provider.FetchSingle(url);

            _series.AddChapter(response, provider.ProviderFor().For, opts.Output);

            // Create progress bar
            var internalProgress = _progressService.NestToMaster(response.TotalPages, $"downloading: {response.Id}");
            _download.HandleOnProgress((_, e) =>
            {
                internalProgress.Tick($"{e.Message}: {response.Id}");
            });

            // Start downloading
            await _download.Start(_series.GetSeries().GetChapters().First());
            await _series.Close(opts.Pack ? internalProgress : null, false);

            progress.Tick();
        }
    }
}
