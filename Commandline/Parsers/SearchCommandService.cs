using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Api.Queries;
using asuka.Core.Chaptering;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Mappings;
using asuka.Core.Requests;
using asuka.Output.ProgressService;
using asuka.Output.Writer;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class SearchCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IValidator<SearchOptions> _validator;
    private readonly IConsoleWriter _console;
    private readonly IDownloader _download;
    private readonly IProgressService _progressService;
    private readonly IPackArchiveToCbz _pack;
    private readonly ISeriesFactory _series;

    public SearchCommandService(
        IGalleryRequestService api,
        IValidator<SearchOptions> validator,
        IConsoleWriter console,
        IDownloader download,
        IProgressService progressService,
        IPackArchiveToCbz pack,
        ISeriesFactory series)
    {
        _api = api;
        _validator = validator;
        _console = console;
        _download = download;
        _progressService = progressService;
        _pack = pack;
        _series = series;
    }

    public async Task RunAsync(object options)
    {
        var opts = (SearchOptions)options;
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            _console.ValidationErrors(validationResult.Errors);
            return;
        }

        // Construct search query
        var searchQueries = new List<string>();
        searchQueries.AddRange(opts.Queries);
        searchQueries.AddRange(opts.Exclude.Select(q => $"-{q}"));
        searchQueries.AddRange(opts.DateRange.Select(d => $"uploaded:{d}"));
        searchQueries.AddRange(opts.PageRange.Select(p => $"pages:{p}"));

        var query = new SearchQuery
        {
            Queries = string.Join(" ", searchQueries),
            PageNumber = opts.Page,
            Sort = opts.Sort
        };

        var responses = await _api.SearchAsync(query);
        if (responses.Count < 1)
        {
            _console.ErrorLine("No results found.");
            return;
        }

        var selection = responses.FilterByUserSelected();

        // Initialise the Progress bar.
        _progressService.CreateMasterProgress(selection.Count, "[task] search download");
        var progress = _progressService.GetMasterProgress();
        
        foreach (var response in selection)
        {
            _series.AddChapter(response, opts.Output, 1);

            var innerProgress = _progressService.NestToMaster(response.TotalPages, $"downloading id: {response.Id}");
            _download.HandleOnDownloadComplete((_, e) =>
            {
                innerProgress.Tick($"{e.Message} id: {response.Id}");
            });

            await _download.Start(_series.GetSeries().Chapters.First());
            await _series.Close();
            
            if (opts.Pack)
            {
                await _pack.RunAsync(_series.GetSeries().Output, opts.Output, innerProgress);
            }
            
            _series.Reset();
            progress.Tick();
        }
    }
}
