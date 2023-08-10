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

public class RecommendCommandService : ICommandLineParser
{
    private readonly ProviderResolverService _provider;
    private readonly IValidator<RecommendOptions> _validator;
    private readonly IProgressProviderFactory _progressFactory;
    private readonly ILogger<RecommendCommandService> _logger;
    private readonly IConsoleWriter _console;
    private readonly AsukaConfiguration _config;

    public RecommendCommandService(
        ProviderResolverService provider,
        IValidator<RecommendOptions> validator,
        IProgressProviderFactory progressFactory,
        ILogger<RecommendCommandService> logger,
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
        var opts = (RecommendOptions)options;
        _logger.LogInformation("RecommenedCommandService called with opts: {@Opts}", opts);
        
        var validator = await _validator.ValidateAsync(opts);
        if (!validator.IsValid)
        {
            _logger.LogError("RecommendedCommandService fails with errors: {@Errors}", validator.Errors);
            validator.Errors.PrintErrors(_console);
            return;
        }
        
        // Find appropriate provider.
        var provider = _provider.GetProviderByUrl(opts.Input) ?? _provider.GetProviderByName(opts.Provider);
        if (provider is null)
        {
            _console.WriteError($"No such {opts.Provider} found.");
            return;
        }

        await ExecuteCommand(opts, provider);
    }

    private async Task ExecuteCommand(RecommendOptions opts, Provider provider)
    {
        var responses = await provider.Api.FetchRecommended(opts.Input);
        
        var selection = await Selection.MultiSelect(responses);
        _logger.LogInformation("Selected items: {@Selection}", selection);

        // Initialise the Progress bar.
        var progress = _progressFactory.Create(selection.Count, $"recommend from: {opts.Input}");
        
        foreach (var response in selection)
        {
            await DownloadList(opts, provider.ImageApi, response, progress);
            progress.Tick();
        }
    }

    private async Task DownloadList(RecommendOptions opts,
        IGalleryImageRequestService imageProvider,
        GalleryResult response,
        IProgressProvider progress)
    {
        var series = new SeriesBuilder()
            .AddChapter(response, imageProvider, 1)
            .SetOutput(opts.Output)
            .Build(_config.DefaultLanguageTitle.ToString());
        _logger.LogInformation("Series built: {@Series}", series);

        var childProgress = progress.Spawn(response.TotalPages, $"downloading id: {response.Id}");
        var downloader = new DownloaderBuilder()
            .AttachLogger(_logger)
            .SetChapter(series.Chapters[0])
            .SetOutput(series.Output)
            .SetEachCompleteHandler(e =>
            {
                childProgress.Tick($"{e.Message}: {response.Id}");
            })
            .Build();

        await downloader.Start();

        if (opts.Pack)
        {
            await CompressAction.Compress(series, opts.Output, childProgress, _logger);
        }
        
        childProgress.Close();
    }
}
