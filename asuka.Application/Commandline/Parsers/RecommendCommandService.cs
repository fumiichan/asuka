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

public class RecommendCommandService : ICommandLineParser
{
    private readonly IValidator<RecommendOptions> _validator;
    private readonly IEnumerable<IGalleryRequestService> _apis;
    private readonly IEnumerable<IGalleryImageRequestService> _imageApis;
    private readonly IProgressProviderFactory _progressFactory;
    private readonly ILogger _logger;

    public RecommendCommandService(
        IValidator<RecommendOptions> validator,
        IEnumerable<IGalleryRequestService> apis,
        IEnumerable<IGalleryImageRequestService> imageApis,
        IProgressProviderFactory progressFactory,
        ILogger logger)
    {
        _validator = validator;
        _apis = apis;
        _imageApis = imageApis;
        _progressFactory = progressFactory;
        _logger = logger;
    }
    
    public async Task Run(object options)
    {
        var opts = (RecommendOptions)options;
        
        // Find appropriate provider.
        var provider = _apis.GetWhatMatches(opts.Input, opts.Provider);
        var imageProvider = _imageApis
            .FirstOrDefault(x => x.ProviderFor().For == opts.Provider);
        if (provider is null || imageProvider is null)
        {
            _logger.LogError("No such {ProviderName} found.", opts.Provider);
            return;
        }

        await ExecuteCommand(opts, provider, imageProvider);
    }

    private async Task ExecuteCommand(RecommendOptions opts, IGalleryRequestService provider, IGalleryImageRequestService imageProvider)
    {
        var validator = await _validator.ValidateAsync(opts);
        if (!validator.IsValid)
        {
            validator.Errors.PrintErrors(_logger);
            return;
        }

        var responses = await provider.FetchRecommended(opts.Input);
        var selection = await Selection.MultiSelect(responses);

        // Initialise the Progress bar.
        var progress = _progressFactory.Create(selection.Count, $"recommend from: {opts.Input}");
        
        foreach (var response in selection)
        {
            await DownloadList(opts, provider, imageProvider, response, progress);
            progress.Tick();
        }
    }

    private static async Task DownloadList(RecommendOptions opts,
        IGalleryRequestService provider,
        IGalleryImageRequestService imageProvider,
        GalleryResult response,
        IProgressProvider progress)
    {
        var series = new SeriesBuilder()
            .AddChapter(response, provider.ProviderFor().For, 1)
            .SetOutput(opts.Output)
            .Build();

        var childProgress = progress.Spawn(response.TotalPages, $"downloading id: {response.Id}");
        var downloader = new DownloaderBuilder()
            .SetImageRequestService(imageProvider)
            .SetChapter(series.Chapters[0])
            .SetOutput(series.Output)
            .SetEachCompleteHandler(e =>
            {
                childProgress.Tick($"{e.Message}: {response.Id}");
            })
            .Build();

        await downloader.Start();
        await series.Chapters[0].Data.WriteJsonMetadata(series.Output);

        if (opts.Pack)
        {
            await CompressAction.Compress(series, childProgress, opts.Output);
        }
        
        childProgress.Close();
    }
}
