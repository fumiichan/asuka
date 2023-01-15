using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Core.Api.Queries;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Mappings;
using asuka.Core.Requests;
using asuka.Output.ProgressService;
using asuka.Output.Writer;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class SearchCommandService : ISearchCommandService
{
    private readonly IGalleryRequestService _api;
    private readonly IValidator<SearchOptions> _validator;
    private readonly IConsoleWriter _console;
    private readonly IDownloadService _download;
    private readonly IProgressService _progressService;
    private readonly IPackArchiveToCbz _pack;

    public SearchCommandService(
        IGalleryRequestService api,
        IValidator<SearchOptions> validator,
        IConsoleWriter console,
        IDownloadService download,
        IProgressService progressService,
        IPackArchiveToCbz pack)
    {
        _api = api;
        _validator = validator;
        _console = console;
        _download = download;
        _progressService = progressService;
        _pack = pack;
    }

    public async Task RunAsync(SearchOptions opts)
    {
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
            var result = await _download.DownloadAsync(response, opts.Output);
            if (opts.Pack)
            {
                await _pack.RunAsync(result.DestinationPath, opts.Output, result.ProgressBar);
            }
            progress.Tick();
        }
    }
}