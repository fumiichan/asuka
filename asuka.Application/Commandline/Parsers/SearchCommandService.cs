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

public class SearchCommandService : ICommandLineParser
{
    private readonly IEnumerable<IGalleryRequestService> _apis;
    private readonly IValidator<SearchOptions> _validator;
    private readonly IDownloader _download;
    private readonly IProgressService _progressService;
    private readonly ISeriesFactory _series;
    private readonly ILogger _logger;

    public SearchCommandService(
        IEnumerable<IGalleryRequestService> apis,
        IValidator<SearchOptions> validator,
        IDownloader download,
        IProgressService progressService,
        ISeriesFactory series,
        ILogger logger)
    {
        _apis = apis;
        _validator = validator;
        _download = download;
        _progressService = progressService;
        _series = series;
        _logger = logger;
    }
    
    public async Task Run(object options)
    {
        var opts = (SearchOptions)options;
        
        // Find appropriate provider.
        var provider = _apis.GetFirst(opts.Provider);
        if (provider is null)
        {
            _logger.LogError("No such {ProviderName} found.", opts.Provider);
            return;
        }

        await ExecuteCommand(opts, provider);
    }

    public async Task ExecuteCommand(SearchOptions opts, IGalleryRequestService provider)
    {
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintErrors(_logger);
            return;
        }

        // Construct search query
        var searchQueries = new List<string>();
        searchQueries.AddRange(opts.Queries);
        searchQueries.AddRange(opts.Exclude.Select(q => $"-{q}"));
        searchQueries.AddRange(opts.DateRange.Select(d => $"uploaded:{d}"));
        searchQueries.AddRange(opts.PageRange.Select(p => $"pages:{p}"));

        var responses = await provider.Search(
            string.Join(" ", searchQueries), opts.Sort, opts.Page);
        if (responses.Count < 1)
        {
            _logger.LogError("No results found.");
            return;
        }

        // Do selection task.
        var selection = await Selection.MultiSelect(responses);

        // Initialise the Progress bar.
        _progressService.CreateMasterProgress(selection.Count, "[task] search download");
        var progress = _progressService.GetMasterProgress();
        
        foreach (var response in selection)
        {
            _series.AddChapter(response, provider.ProviderFor().For, opts.Output, 1);

            var innerProgress = _progressService.NestToMaster(response.TotalPages, $"downloading id: {response.Id}");
            _download.HandleOnProgress((_, e) =>
            {
                innerProgress.Tick($"{e.Message} id: {response.Id}");
            });

            await _download.Start(_series.GetSeries().Chapters.First());
            await _series.Close(opts.Pack ? innerProgress : null, false);
            
            progress.Tick();
        }
    }
}
