using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using asuka.Api;
using asuka.Commandline.Options;
using asuka.Api.Queries;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Extensions;
using asuka.Core.Mappings;
using asuka.Core.Requests;
using asuka.Core.Utilities;
using asuka.Output;
using asuka.Output.Progress;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class SearchCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IGalleryImage _apiImage;
    private readonly IValidator<SearchOptions> _validator;
    private readonly IProgressFactory _progress;

    public SearchCommandService(
        IGalleryRequestService api,
        IGalleryImage apiImage,
        IValidator<SearchOptions> validator,
        IProgressFactory progress)
    {
        _api = api;
        _apiImage = apiImage;
        _validator = validator;
        _progress = progress;
    }

    public async Task RunAsync(object options)
    {
        var opts = (SearchOptions)options;
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintValidationExceptions();
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
            Console.WriteLine("No results found.");
            return;
        }

        var selection = responses.FilterByUserSelected();

        // Initialise the Progress bar.
        var mainProgress = _progress.Create(selection.Count, "Downloading found results...");
        
        foreach (var response in selection)
        {
            var childProgress = mainProgress
                .Spawn(response.TotalPages, $"Downloading {response.Title.GetTitle()}")!;
            var output = Path.Combine(opts.Output, PathUtils.NormalizeName(response.Title.GetTitle()));
            var downloader = new DownloadBuilder(response, 1)
            {
                Output = output,
                Request = _apiImage,
                OnEachComplete = _ =>
                {
                    childProgress.Tick();
                },
                OnComplete = async data =>
                {
                    await data.WriteMetadata(Path.Combine(output, "details.json"));
                    if (opts.Pack)
                    {
                        await Compress.ToCbz(output, childProgress);
                    }
                }
            };

            await downloader.Start();
            mainProgress.Tick();
        }
    }
}
