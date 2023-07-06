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
using asuka.Core.Requests;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers;

internal class DownloadTaskArguments
{
    public string Input { get; init; }
    public bool Pack { get; init; }
    public bool ReadOnly { get; init; }
    public string OutputPath { get; init; }
}

public class GetCommandService : ICommandLineParser
{
    private readonly IEnumerable<IGalleryRequestService> _apis;
    private readonly IEnumerable<IGalleryImageRequestService> _imageApis;
    private readonly IValidator<GetOptions> _validator;
    private readonly IProgressProviderFactory _progressFactory;
    private readonly ILogger _logger;

    public GetCommandService(
        IEnumerable<IGalleryRequestService> apis,
        IEnumerable<IGalleryImageRequestService> imageApis,
        IValidator<GetOptions> validator,
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
        var opts = (GetOptions)options;
        
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintErrors(_logger);
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
            var provider = _apis.GetWhatMatches(code, opts.Provider);
            if (provider is null)
            {
                _logger.LogError("Unable to determine provider for {code}", code);
                continue;
            }
            
            await DownloadTask(provider, new DownloadTaskArguments
            {
                Input = code,
                Pack = opts.Pack,
                ReadOnly = opts.ReadOnly,
                OutputPath = opts.Output
            });
        }
    }

    private async Task DownloadTask(IGalleryRequestService api, DownloadTaskArguments args)
    {
        var response = await api.FetchSingle(args.Input);
        _logger.LogInformation(response.BuildReadableInformation());

        // Don't download.
        if (args.ReadOnly)
        {
            return;
        }

        var series = new SeriesBuilder()
            .AddChapter(response, api.ProviderFor().For)
            .SetOutput(args.OutputPath)
            .Build();

        var imageApi = _imageApis
            .FirstOrDefault(x => x.ProviderFor().For == series.Chapters[0].Source);

        var progress = _progressFactory.Create(response.TotalPages, $"downloading: {response.Id}");

        var downloader = new DownloaderBuilder()
            .SetImageRequestService(imageApi)
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
            await CompressAction.Compress(series, progress, args.OutputPath);
        }
        
        progress.Close();
    }
}
