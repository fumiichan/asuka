using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Utilities;
using asuka.Core.Chaptering;
using asuka.Core.Configuration;
using asuka.Core.Downloader;
using asuka.Core.Output.Progress;
using asuka.Core.Requests;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

internal class HandleArrayTaskArgs
{
    public IList<string> Codes { get; init; }
    public string Output { get; init; }
    public bool Pack { get; init; }
    public string ProviderName { get; init; }
}

public class SeriesCreatorCommandService : ICommandLineParser
{
    private readonly IEnumerable<IGalleryRequestService> _apis;
    private readonly IDownloader _downloader;
    private readonly IProgressService _progress;
    private readonly IConfigurationManager _config;
    private readonly IValidator<SeriesCreatorCommandOptions> _validator;
    private readonly ISeriesFactory _series;
    private readonly ILogger _logger;

    public SeriesCreatorCommandService(
        IEnumerable<IGalleryRequestService> apis,
        IDownloader downloader,
        IProgressService progress,
        IConfigurationManager config,
        IValidator<SeriesCreatorCommandOptions> validator,
        ISeriesFactory series,
        ILogger logger)
    {
        _apis = apis;
        _downloader = downloader;
        _progress = progress;
        _config = config;
        _validator = validator;
        _series = series;
        _logger = logger;
    }
    
    public async Task Run(object options)
    {
        var opts = (SeriesCreatorCommandOptions)options;
        
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintErrors(_logger);
            return;
        }

        await ExecuteCommand(opts);
    }

    private async Task ExecuteCommand(SeriesCreatorCommandOptions opts)
    {
        // Temporarily enable tachiyomi folder layout
        _config.SetValue("layout.tachiyomi", "yes");

        var list = opts.FromList.ToList();
        await HandleArrayTask(new HandleArrayTaskArgs
        {
            Codes = list,
            Output = opts.Output,
            Pack = opts.Pack,
            ProviderName = opts.Provider
        });
    }

    private async Task HandleArrayTask(HandleArrayTaskArgs args)
    {
        // Queue list of chapters.
        for (var i = 0; i < args.Codes.Count; i++)
        {
            var provider = _apis.GetWhatMatches(args.Codes[i], args.ProviderName);
            
            try
            {
                var response = await provider.FetchSingle(args.Codes[i]);
                _series.AddChapter(response, provider.ProviderFor().For, args.Output, i + 1);
            }
            catch
            {
                _logger.LogWarning($"Skipping: {args.Codes[i]} because of an error.");
            }
        }

        // If there's no chapters (due to likely most of them failed to fetch metadata)
        // Quit immediately.
        if (_series.GetSeries() == null)
        {
            _logger.LogInformation("Nothing to do. Quitting...");
            return;
        }

        // Download chapters.
        var chapters = _series.GetSeries().Chapters;
        _progress.CreateMasterProgress(args.Codes.Count, "downloading series");

        foreach (var chapter in chapters)
        {
            try
            {
                var innerProgress =
                    _progress.NestToMaster(chapter.Data.TotalPages, $"downloading chapter {chapter.ChapterId}");
                _downloader.HandleOnProgress((_, _) =>
                {
                    innerProgress.Tick();
                });

                await _downloader.Start(chapter);
            }
            finally
            {
                _progress.GetMasterProgress().Tick();
            }
        }

        await _series.Close(args.Pack ? _progress.GetMasterProgress() : null);
    }
}
