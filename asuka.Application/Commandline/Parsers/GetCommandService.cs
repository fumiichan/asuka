using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Commandline.Parsers.Common;
using asuka.Application.Output.ConsoleWriter;
using asuka.Application.Output.Progress;
using asuka.Application.Services;
using asuka.Application.Utilities;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Sdk.Providers.Extensions;
using asuka.Sdk.Providers.Requests;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

internal class DownloadTaskArguments
{
    public string Input { get; init; }
    public bool Pack { get; init; }
    public bool ReadOnly { get; init; }
    public string OutputPath { get; init; }
    public IGalleryImageRequestService ImageApi { get; init; }
    public IGalleryRequestService Api { get; init; }
}

public class GetCommandService : ICommandLineParser
{
    private readonly ProviderResolverService _provider;
    private readonly IValidator<GetOptions> _validator;
    private readonly IProgressProviderFactory _progressFactory;
    private readonly ILogger<GetCommandService> _logger;
    private readonly IConsoleWriter _console;

    public GetCommandService(
        ProviderResolverService provider,
        IValidator<GetOptions> validator,
        IProgressProviderFactory progressFactory,
        ILogger<GetCommandService> logger,
        IConsoleWriter console)
    {
        _provider = provider;
        _validator = validator;
        _progressFactory = progressFactory;
        _logger = logger;
        _console = console;
    }

    public async Task Run(object options)
    {
        var opts = (GetOptions)options;
        _logger.LogInformation("GetCommandService called with args: {@Opts}", opts);
        
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("GetCommandService failed with errors: {@Errors}", validationResult.Errors);
            validationResult.Errors.PrintErrors(_console);
            return;
        }

        await ExecuteCommand(opts);
    }

    private async Task ExecuteCommand(GetOptions opts)
    {
        foreach (var code in opts.Input)
        {
            // Allow dynamic provider switching when different providers, otherwise default it to specified
            // provider if not full URL is supplied.
            var provider = _provider.GetProviderByUrl(code) ?? _provider.GetProviderByName(opts.Provider);
            if (provider is null)
            {
                _console.WriteError($"Unable to determine provider for {code}");
                continue;
            }
            
            await DownloadTask(new DownloadTaskArguments
            {
                Input = code,
                Pack = opts.Pack,
                ReadOnly = opts.ReadOnly,
                OutputPath = opts.Output,
                Api = provider.Api,
                ImageApi = provider.ImageApi
            });
        }
    }

    private async Task DownloadTask(DownloadTaskArguments args)
    {
        var response = await args.Api.FetchSingle(args.Input);
        if (response is null)
        {
            _console.WriteError($"Unable to get image for input: {args.Input}");
            return;
        }

        _console.Write(response.BuildReadableInformation());

        // Don't download.
        if (args.ReadOnly)
        {
            return;
        }
        
        var series = new SeriesBuilder()
            .AddChapter(response, args.ImageApi)
            .SetOutput(args.OutputPath)
            .Build();
        _logger.LogInformation("Series built: {@Series}", series);

        var progress = _progressFactory.Create(response.TotalPages, $"downloading: {response.Id}");
        var downloader = new DownloaderBuilder()
            .AttachLogger(_logger)
            .SetChapter(series.Chapters[0])
            .SetOutput(series.Output)
            .SetEachCompleteHandler(e =>
            {
                progress.Tick($"{e.Message}: {response.Id}");
            })
            .Build();

        await downloader.Start();
        await series.Chapters[0].Data.WriteJsonMetadata(series.Output);
        
        if (args.Pack)
        {
            await CompressAction.Compress(series, args.OutputPath, progress, _logger);
        }

        progress.Close();
    }
}
