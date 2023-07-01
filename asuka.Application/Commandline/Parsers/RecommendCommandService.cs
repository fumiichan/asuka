using System.Collections.Generic;
using System.Linq;
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

public class RecommendCommandService : ICommandLineParser
{
    private readonly IValidator<RecommendOptions> _validator;
    private readonly IEnumerable<IGalleryRequestService> _apis;
    private readonly IDownloader _download;
    private readonly IProgressService _progressService;
    private readonly ISeriesFactory _series;
    private readonly ILogger _logger;

    public RecommendCommandService(
        IValidator<RecommendOptions> validator,
        IEnumerable<IGalleryRequestService> apis,
        IDownloader download,
        IProgressService progressService,
        ISeriesFactory series,
        ILogger logger)
    {
        _validator = validator;
        _apis = apis;
        _download = download;
        _progressService = progressService;
        _series = series;
        _logger = logger;
    }
    
    public async Task Run(object options)
    {
        var opts = (RecommendOptions)options;
        
        // Find appropriate provider.
        var provider = _apis.GetWhatMatches(opts.Input, opts.Provider);
        if (provider is null)
        {
            _logger.LogError("No such {ProviderName} found.", opts.Provider);
            return;
        }

        await ExecuteCommand(opts, provider);
    }

    private async Task ExecuteCommand(RecommendOptions opts, IGalleryRequestService provider)
    {
        var validator = await _validator.ValidateAsync(opts);
        if (!validator.IsValid)
        {
            validator.Errors.PrintErrors(_logger);
            return;
        }

        var responses = await provider.FetchRecommended(opts.Input);
        var selection = await Selection.MultiSelect(responses);

        // Initialise the Progress bar.
        _progressService.CreateMasterProgress(selection.Count, $"[task] recommend from id: {opts.Input}");
        var progress = _progressService.GetMasterProgress();

        foreach (var response in selection)
        {
            _series.AddChapter(response, provider.ProviderFor().For, opts.Output, 1);

            var innerProgress = _progressService.NestToMaster(response.TotalPages, $"downloading id: {response.Id}");
            _download.HandleOnProgress((_, e) =>
            {
                innerProgress.Tick($"{e.Message} id: {response.Id}");
            });

            await _download.Start(_series.GetSeries().GetChapters().FirstOrDefault());
            await _series.Close(opts.Pack ? innerProgress : null, false);
            
            progress.Tick();
        }
    }
}
