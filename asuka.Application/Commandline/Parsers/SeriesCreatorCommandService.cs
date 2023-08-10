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
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

public class SeriesCreatorCommandService : ICommandLineParser
{
    private readonly ProviderResolverService _provider;
    private readonly IProgressProviderFactory _progressFactory;
    private readonly IValidator<SeriesCreatorCommandOptions> _validator;
    private readonly ILogger<SeriesCreatorCommandService> _logger;
    private readonly IConsoleWriter _console;
    private readonly AsukaConfiguration _config;

    public SeriesCreatorCommandService(
        ProviderResolverService provider,
        IProgressProviderFactory progressFactory,
        IValidator<SeriesCreatorCommandOptions> validator,
        ILogger<SeriesCreatorCommandService> logger,
        IConsoleWriter console,
        AsukaConfiguration config)
    {
        _provider = provider;
        _progressFactory = progressFactory;
        _validator = validator;
        _logger = logger;
        _console = console;
        _config = config;
    }
    
    public async Task Run(object options)
    {
        var opts = (SeriesCreatorCommandOptions)options;
        _logger.LogInformation("SeriesCreatorCommandService called with opts {@Opts}", opts);
        
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            _logger.LogError("SeriesCreatorCommandService fails with errors: {@Errors}", validationResult.Errors);

            validationResult.Errors.PrintErrors(_console);
            return;
        }

        await ExecuteCommand(opts);
    }

    private async Task ExecuteCommand(SeriesCreatorCommandOptions args)
    {
        // Queue list of chapters.
        var codes = args.FromList.ToList();
        var seriesBuilder = new SeriesBuilder();
        for (var i = args.StartOffset; i < args.StartOffset + codes.Count; i++)
        {
            var realIndex = (codes.Count + i) - (args.StartOffset + codes.Count);
            var provider = _provider.GetProviderByUrl(codes[realIndex]) ?? _provider.GetProviderByName(args.Provider);

            try
            {
                var response = await provider.Api.FetchSingle(codes[realIndex]);
                seriesBuilder.AddChapter(response, provider.ImageApi, i);
            }
            catch
            {
                _console.WriteWarning($"Skipping: {codes[realIndex]} because of an error.");
            }
        }

        seriesBuilder.SetOutput(args.Output);
        var series = seriesBuilder.Build(_config.DefaultLanguageTitle.ToString());
        _logger.LogInformation("Series Built: {@Series}", series);

        // If there's no chapters (due to likely most of them failed to fetch metadata)
        // Quit immediately.
        if (series.Chapters.Count == 0)
        {
            _logger.LogWarning("Nothing to do. chapter count = {Count}", series.Chapters.Count);
            _console.WriteInformation("Nothing to do. Quitting...");
            return;
        }

        // Download chapters.
        var progress = _progressFactory.Create(series.Chapters.Count, "downloading series...");

        foreach (var chapter in series.Chapters)
        {
            try
            {
                var childProgress = progress.Spawn(chapter.Data.TotalPages, $"downloading chapter {chapter.Id}");
                var downloader = new DownloaderBuilder()
                    .AttachLogger(_logger)
                    .SetChapter(chapter)
                    .SetOutput(series.Output)
                    .SetEachCompleteHandler(_ =>
                    {
                        childProgress.Tick();
                    })
                    .Build();

                await downloader.Start();
                childProgress.Close();
            }
            finally
            {
                progress.Tick();
            }
        }

        if (args.Pack)
        {
            await CompressAction.Compress(series, args.Output, progress, _logger);
        }

        // Cleanup
        progress.Close();
    }
}
