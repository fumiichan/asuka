using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Commandline.Parsers.Common;
using asuka.Application.Output.Progress;
using asuka.Application.Utilities;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Core.Extensions;
using asuka.Core.Models;
using asuka.Core.Requests;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

public class SearchCommandService : ICommandLineParser
{
    private readonly IEnumerable<IGalleryRequestService> _apis;
    private readonly IEnumerable<IGalleryImageRequestService> _imageApis;
    private readonly IValidator<SearchOptions> _validator;
    private readonly IProgressProviderFactory _progressFactory;
    private readonly ILogger _logger;

    public SearchCommandService(
        IEnumerable<IGalleryRequestService> apis,
        IEnumerable<IGalleryImageRequestService> imageApis,
        IValidator<SearchOptions> validator,
        IProgressProviderFactory progressFactory,
        ILogger logger)
    {
        _apis = apis;
        _imageApis = imageApis;
        _validator = validator;
        _progressFactory = progressFactory;
        _logger = logger;
    }
    
    public async Task Run(object options)
    {
        var opts = (SearchOptions)options;
        
        // Find appropriate provider.
        var provider = _apis.GetFirst(opts.Provider);
        var imageProvider = _imageApis.FirstOrDefault(x => x.ProviderFor().For == opts.Provider);
        if (provider is null || imageProvider is null)
        {
            _logger.LogError("No such {ProviderName} found.", opts.Provider);
            return;
        }

        await ExecuteCommand(opts, provider, imageProvider);
    }

    public async Task ExecuteCommand(SearchOptions opts, IGalleryRequestService provider, IGalleryImageRequestService imageProvider)
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
        var progress = _progressFactory.Create(selection.Count, "downloading selected items...");
        
        foreach (var response in selection)
        {
            await DownloadList(opts, provider, imageProvider, response, progress);
            progress.Tick();
        }
    }

    private static async Task DownloadList(SearchOptions opts,
        IGalleryRequestService provider,
        IGalleryImageRequestService imageProvider,
        GalleryResult response,
        IProgressProvider progress)
    {
        var series = new SeriesBuilder()
            .AddChapter(response, provider.ProviderFor().For, 1)
            .SetOutput(opts.Output)
            .Build();

        var innerProgress = progress.Spawn(response.TotalPages, $"downloading id: {response.Id}");
        var downloader = new DownloaderBuilder()
            .SetImageRequestService(imageProvider)
            .SetChapter(series.Chapters[0])
            .SetOutput(series.Output)
            .SetEachCompleteHandler(e =>
            {
                innerProgress.Tick($"{e.Message}: {response.Id}");
            })
            .Build();

        await downloader.Start();
        await series.Chapters[0].Data.WriteJsonMetadata(series.Output);

        if (opts.Pack)
        {
            await CompressAction.Compress(series, innerProgress, opts.Output);
        }

        innerProgress.Close();
    }
}
