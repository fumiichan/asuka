using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using ShellProgressBar;
using asuka.Api.Queries;
using asuka.CommandOptions;
using asuka.Downloader;
using asuka.Mappings;
using asuka.Output;
using asuka.Services;
using asuka.Utils;

namespace asuka.CommandParsers
{
    public class SearchCommandService : ISearchCommandService
    {
        private readonly IGalleryRequestService _api;
        private readonly IValidator<SearchOptions> _validator;
        private readonly IConsoleWriter _console;
        private readonly IDownloadService _download;

        public SearchCommandService(
            IGalleryRequestService api,
            IValidator<SearchOptions> validator,
            IConsoleWriter console,
            IDownloadService download)
        {
            _api = api;
            _validator = validator;
            _console = console;
            _download = download;
        }

        public async Task RunAsync(SearchOptions opts)
        {
            var validationResult = await _validator.ValidateAsync(opts);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    _console.ErrorLine($"Validation Failure: {error.ErrorMessage}");
                }
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
                PageNumber = 1,
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
            using var progress = new ProgressBar(selection.Count, $"[task] search download",
                ProgressBarConfiguration.BarOption);

            foreach (var response in selection)
            {
                await _download.DownloadAsync(response, opts.Output, opts.Pack, progress);
                progress.Tick();
            }
        }
    }
}
