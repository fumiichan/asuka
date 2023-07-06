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

public class SeriesCreatorCommandService : ICommandLineParser
{
    private readonly IEnumerable<IGalleryRequestService> _apis;
    private readonly IEnumerable<IGalleryImageRequestService> _imageApis;
    private readonly IProgressProviderFactory _progressFactory;
    private readonly IValidator<SeriesCreatorCommandOptions> _validator;
    private readonly ILogger _logger;

    public SeriesCreatorCommandService(
        IEnumerable<IGalleryRequestService> apis,
        IEnumerable<IGalleryImageRequestService> imageApis,
        IProgressProviderFactory progressFactory,
        IValidator<SeriesCreatorCommandOptions> validator,
        ILogger logger)
    {
        _apis = apis;
        _imageApis = imageApis;
        _progressFactory = progressFactory;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task Run(object options)
    {
        var opts = (SeriesCreatorCommandOptions)options;
        
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintErrors(_logger);
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
            var provider = _apis.GetWhatMatches(codes[realIndex], args.Provider);

            try
            {
                var response = await provider.FetchSingle(codes[realIndex]);
                seriesBuilder.AddChapter(response, provider.ProviderFor().For, i);
            }
            catch
            {
                _logger.LogWarning("Skipping: {codeIndex} because of an error.", codes[realIndex]);
            }
        }

        seriesBuilder.SetOutput(args.Output);
        var series = seriesBuilder.Build();

        // If there's no chapters (due to likely most of them failed to fetch metadata)
        // Quit immediately.
        if (series.Chapters.Count == 0)
        {
            _logger.LogInformation("Nothing to do. Quitting...");
            return;
        }

        // Download chapters.
        var progress = _progressFactory.Create(series.Chapters.Count, "downloading series...");

        foreach (var chapter in series.Chapters)
        {
            try
            {
                var childProgress = progress.Spawn(chapter.Data.TotalPages, $"downloading chapter {chapter.Id}");
                var imageApi = _imageApis
                    .FirstOrDefault(x => x.ProviderFor().For == chapter.Source);
                var downloader = new DownloaderBuilder()
                    .SetImageRequestService(imageApi)
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

        await series.Chapters[0].Data.WriteJsonMetadata(series.Output);

        if (args.Pack)
        {
            await CompressAction.Compress(series, progress, args.Output);
        }

        // Cleanup
        progress.Close();
    }
}
