using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

public class FileCommandService : ICommandLineParser
{
    private readonly IEnumerable<IGalleryRequestService> _apis;
    private readonly IEnumerable<IGalleryImageRequestService> _imageApis;
    private readonly IProgressProviderFactory _progressFactory;
    private readonly IValidator<FileCommandOptions> _validator;
    private readonly ILogger _logger;

    public FileCommandService(
        IEnumerable<IGalleryRequestService> apis,
        IEnumerable<IGalleryImageRequestService> imageApis,
        IProgressProviderFactory progressFactory,
        IValidator<FileCommandOptions> validator,
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
        var opts = (FileCommandOptions)options;
        
        var validationResult = await _validator.ValidateAsync(opts);

        if (!validationResult.IsValid)
        {
            validationResult.Errors.PrintErrors(_logger);
            return;
        }

        await ExecuteCommand(opts);
    }

    private async Task ExecuteCommand(FileCommandOptions opts)
    {
        var textFile = await File.ReadAllLinesAsync(opts.FilePath, Encoding.UTF8)
            .ConfigureAwait(false);

        var progress = _progressFactory.Create(textFile.Length, "downloading from text file...");

        foreach (var url in textFile)
        {
            await DownloadEach(opts, url, progress);
        }
        
        progress.Close();
    }

    private async Task DownloadEach(FileCommandOptions opts, string url, IProgressProvider progress)
    {
        // Allows dynamic provider switching to allow multiple providers to be in single file
        var provider = _apis.GetFirstByHostname(url);
        if (provider is null)
        {
            var failProgress = progress.Spawn(1, $"skipped: {url}");
            failProgress.Tick();
            failProgress.Close();
            progress.Tick();
            return;
        }

        var response = await provider.FetchSingle(url);

        var series = new SeriesBuilder()
            .AddChapter(response, provider.ProviderFor().For)
            .SetOutput(opts.Output)
            .Build();

        // Create progress bar
        var internalProgress = progress.Spawn(response.TotalPages, $"downloading: {response.Id}");

        var imageApi = _imageApis
            .FirstOrDefault(x => x.ProviderFor().For == provider.ProviderFor().For);
        var downloader = new DownloaderBuilder()
            .SetImageRequestService(imageApi)
            .SetChapter(series.Chapters[0])
            .SetOutput(series.Output)
            .SetEachCompleteHandler(e =>
            {
                internalProgress.Tick($"{e.Message}: {response.Id}");
            })
            .Build();

        // Start downloading
        await downloader.Start();
        await series.Chapters[0].Data.WriteJsonMetadata(series.Output);

        // Compression
        if (opts.Pack)
        {
            await CompressAction.Compress(series, internalProgress, opts.Output);
        }

        // Cleanup
        progress.Tick();
        internalProgress.Close();
    }
}
