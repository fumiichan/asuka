using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Api.Queries;
using asuka.Application.Commandline.Options;
using asuka.Application.Output.Writer;
using asuka.Application.Utilities;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Core.Output.Progress;
using asuka.Core.Requests;
using FluentValidation;

namespace asuka.Application.Commandline.Parsers;

public class SearchCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IValidator<SearchOptions> _validator;
    private readonly IConsoleWriter _console;
    private readonly IDownloader _download;
    private readonly IProgressService _progressService;
    private readonly ISeriesFactory _series;

    public SearchCommandService(
        IGalleryRequestService api,
        IValidator<SearchOptions> validator,
        IConsoleWriter console,
        IDownloader download,
        IProgressService progressService,
        ISeriesFactory series)
    {
        _api = api;
        _validator = validator;
        _console = console;
        _download = download;
        _progressService = progressService;
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

        var queries = new SearchQuery
        {
            Queries = string.Join(" ", searchQueries),
            PageNumber = opts.Page,
            Sort = opts.Sort
        };

        var responses = await _api.Search(queries);
        if (responses.Count < 1)
        {
            _console.ErrorLine("No results found.");
            return;
        }

        // Do selection task.
        var selection = await Selection.MultiSelect(responses);

        // Initialise the Progress bar.
        _progressService.CreateMasterProgress(selection.Count, "[task] search download");
        var progress = _progressService.GetMasterProgress();
        
        foreach (var response in selection)
        {
            _series.AddChapter(response, opts.Output, 1);

            var innerProgress = _progressService.NestToMaster(response.TotalPages, $"downloading id: {response.Id}");
            _download.HandleOnProgress((_, e) =>
            {
                innerProgress.Tick($"{e.Message} id: {response.Id}");
            });

            await _download.Start(_series.GetSeries().Chapters.First());
            await _series.Close(opts.Pack ? innerProgress : null);
            
            progress.Tick();
        }
    }
}
