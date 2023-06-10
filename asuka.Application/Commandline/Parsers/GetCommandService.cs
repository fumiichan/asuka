using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Utilities;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Core.Extensions;
using asuka.Core.Output.Progress;
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
    private readonly IValidator<GetOptions> _validator;
    private readonly IDownloader _download;
    private readonly IProgressService _progress;
    private readonly ISeriesFactory _series;
    private readonly ILogger _logger;

    public GetCommandService(
        IEnumerable<IGalleryRequestService> apis,
        IValidator<GetOptions> validator,
        IDownloader download,
        IProgressService progress,
        ISeriesFactory series,
        ILogger logger)
    {
        _apis = apis;
        _validator = validator;
        _download = download;
        _progress = progress;
        _series = series;
        _logger = logger;
    }

    public async Task Run(object options)
    {
        var opts = (GetOptions)options;
        
        // Find appropriate provider.
        var provider = _apis.GetFirst(opts.Provider);
        if (provider is null)
        {
            _logger.LogError("No such {ProviderName} found.", opts.Provider);
            return;
        }

        await ExecuteCommand(opts, provider);
    }

    private async Task ExecuteCommand(GetOptions opts, IGalleryRequestService provider)
    {
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintErrors(_logger);
            return;
        }

        foreach (var code in opts.Input)
        {
            await DownloadTask(provider, new DownloadTaskArguments
            {
                Input = code.ToString(),
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
        
        _series.AddChapter(response, args.OutputPath);

        _progress.CreateMasterProgress(response.TotalPages, $"downloading: {response.Id}");
        var progress = _progress.GetMasterProgress();

        _download.HandleOnProgress((_, e) =>
        {
            progress.Tick($"{e.Message}: {response.Id}");
        });

        await _download.Start(api.ProviderFor(), _series.GetSeries().Chapters.First());
        await _series.Close(args.Pack ? progress : null);
    }
}
