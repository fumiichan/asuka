using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        
        // Find appropriate provider.
        var provider = _apis.GetFirst(opts.Provider);
        if (provider is null)
        {
            _logger.LogError("No such {ProviderName} found.", opts.Provider);
            return;
        }

        await ExecuteCommand(opts, provider);
    }

    private async Task ExecuteCommand(FileCommandOptions opts, IGalleryRequestService requestService)
    {
        var validationResult = await _validator.ValidateAsync(opts);

        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintErrors(_logger);
            return;
        }

        var textFile = await File.ReadAllLinesAsync(opts.FilePath, Encoding.UTF8)
            .ConfigureAwait(false);
        var validUrls = FilterValidUrls(textFile);

        if (validUrls.Count == 0)
        {
            _logger.LogError("No valid URLs found.");
            return;
        }

        _progressService.CreateMasterProgress(validUrls.Count, "downloading from text file...");
        var progress = _progressService.GetMasterProgress();

        foreach (var url in validUrls)
        {
            var code = new Regex("\\d+").Match(url).Value;
            var response = await requestService.FetchSingle(code);

            _series.AddChapter(response, opts.Output);
            
            // Create progress bar
            var internalProgress = _progressService.NestToMaster(response.TotalPages, $"downloading: {response.Id}");
            _download.HandleOnProgress((_, e) =>
            {
                internalProgress.Tick($"{e.Message}: {response.Id}");
            });

            // Start downloading
            await _download.Start(requestService.ProviderFor(), _series.GetSeries().Chapters.First());
            await _series.Close(opts.Pack ? internalProgress : null);
            
            progress.Tick();
        }
    }

    private static IReadOnlyList<string> FilterValidUrls(IEnumerable<string> urls)
    {
        return urls.Where(url => new Regex("^http(s)?:\\/\\/(nhentai\\.net)\\b([//g]*)\\b([\\d]{1,6})\\/?$").IsMatch(url)).ToList();
    }
}
