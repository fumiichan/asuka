using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Commandline.Parsers.Common;
using asuka.Application.Output.ConsoleWriter;
using asuka.Application.Output.Progress;
using asuka.Application.Services;
using asuka.Application.Services.Configuration;
using asuka.Application.Utilities;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Sdk.Providers.Models;
using asuka.Sdk.Providers.Requests;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

public class SearchCommandService : ICommandLineParser
{
    private readonly ProviderResolverService _provider;
    private readonly IValidator<SearchOptions> _validator;
    private readonly IProgressProviderFactory _progressFactory;
    private readonly ILogger<SearchCommandService> _logger;
    private readonly IConsoleWriter _console;
    private readonly AsukaConfiguration _config;

    public SearchCommandService(
        ProviderResolverService provider,
        IValidator<SearchOptions> validator,
        IProgressProviderFactory progressFactory,
        ILogger<SearchCommandService> logger,
        IConsoleWriter console,
        AsukaConfiguration config)
    {
        _provider = provider;
        _validator = validator;
        _progressFactory = progressFactory;
        _logger = logger;
        _console = console;
        _config = config;
    }
    
    public async Task Run(object options)
    {
        var opts = (SearchOptions)options;
        _logger.LogInformation("SearchCommandService called with opts: {@Opts}", opts);
        
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            _logger.LogError("SearchCommandService fails with errors: {@Errors}", validationResult.Errors);
            
            validationResult.Errors.PrintErrors(_console);
            return;
        }
        
        // Find appropriate provider.
        var provider = _provider.GetProviderByName(opts.Provider);
        if (provider is null)
        {
            _console.WriteError($"No such {opts.Provider} found.");
            return;
        }

        await ExecuteCommand(opts, provider);
    }

    private async Task ExecuteCommand(SearchOptions opts, Provider provider)
    {
        // Construct search query
        var searchQueries = new List<string>();
        searchQueries.AddRange(opts.Queries);
        searchQueries.AddRange(opts.Exclude.Select(q => $"-{q}"));
        searchQueries.AddRange(opts.DateRange.Select(d => $"uploaded:{d}"));
        searchQueries.AddRange(opts.PageRange.Select(p => $"pages:{p}"));

        var responses = await provider.Api.Search(
            string.Join(" ", searchQueries), opts.Sort, opts.Page);
        if (responses.Count < 1)
        {
            _console.WriteError("No results found.");
            return;
        }

        // Do selection task.
        var selection = await Selection.MultiSelect(responses);

        // Initialise the Progress bar.
        var progress = _progressFactory.Create(selection.Count, "downloading selected items...");
        
        foreach (var response in selection)
        {
            _logger.LogInformation("Processing data: {@Response}", response);

            await DownloadList(opts, provider.ImageApi, response, progress);
            progress.Tick();
        }
    }

    private async Task DownloadList(SearchOptions opts,
        IGalleryImageRequestService imageProvider,
        GalleryResult response,
        IProgressProvider progress)
    {
        var series = new SeriesBuilder()
            .AddChapter(response, imageProvider, 1)
            .SetOutput(opts.Output)
            .Build(_config.DefaultLanguageTitle.ToString());
        _logger.LogInformation("Series built: {@Series}", series);

        var innerProgress = progress.Spawn(response.TotalPages, $"downloading id: {response.Id}");
        var downloader = new DownloaderBuilder()
            .AttachLogger(_logger)
            .SetChapter(series.Chapters[0])
            .SetOutput(series.Output)
            .SetEachCompleteHandler(e =>
            {
                innerProgress.Tick($"{e.Message}: {response.Id}");
            })
            .Build();

        await downloader.Start();

        if (opts.Pack)
        {
            await CompressAction.Compress(series, opts.Output, innerProgress, _logger);
        }

        innerProgress.Close();
    }
}
