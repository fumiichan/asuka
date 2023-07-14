using asuka.Core.Chaptering.Models;
using asuka.Core.Events;
using asuka.Core.Models;
using asuka.Core.Requests;
using Microsoft.Extensions.Logging;

namespace asuka.Core.Downloader;

internal class DownloadImageArgs
{
    public string FileName { get; init; }
    public string ImageDownloadPath { get; init; }
}

public sealed class Downloader : ProgressEmittable
{
    private readonly IGalleryImageRequestService _api;
    private readonly Chapter _chapter;
    private readonly string _output;
    private readonly ILogger _logger;

    public Downloader(
        Chapter chapter,
        string outputPath,
        ILogger logger)
    {
        _api = chapter.Requestor;
        _chapter = chapter;
        _output = outputPath;
        _logger = logger;
    }

    public async Task Start()
    {
        _logger.LogInformation("Download started for task: {@Chapter}", _chapter);
        
        var throttler = new SemaphoreSlim(2);
        var taskList = _chapter.Data.Images
            .Select(x => Task.Run(async () =>
            {
                await DownloadTask(throttler, x);
            }));

        await Task.WhenAll(taskList).ConfigureAwait(false);
    }

    private async Task DownloadTask(SemaphoreSlim throttler, GalleryImageResult result)
    {
        await throttler.WaitAsync().ConfigureAwait(false);

        try
        {
            await DownloadImage(_api, new DownloadImageArgs
                {
                    FileName = result.Filename,
                    ImageDownloadPath = result.ServerFilename
                })
                .ConfigureAwait(false);
        }
        finally
        {
            throttler.Release();
        }
    }

    private async Task DownloadImage(IGalleryImageRequestService provider, DownloadImageArgs args)
    {
        var filePath = Path.Combine(_output, args.FileName);
        if (File.Exists(filePath))
        {
            _logger.LogInformation("Progress skipped for url: {Url} due to that it exists", args.ImageDownloadPath);
            
            OnProgressEvent(new ProgressEventArgs("skip existing"));
            return;
        }

        var image = await provider.FetchImage(args.ImageDownloadPath);
        if (image is null)
        {
            _logger.LogError("Image download fail for url: {Url}", args.ImageDownloadPath);
            
            OnProgressEvent(new ProgressEventArgs("download failed"));
            return;
        }
        
        var imageData = await image.ReadAsByteArrayAsync();

        try
        {
            await File.WriteAllBytesAsync(filePath, imageData)
                .ConfigureAwait(false);

            _logger.LogInformation("Download succeeded for url: {Url}", args.ImageDownloadPath);
            OnProgressEvent(new ProgressEventArgs("downloading"));
        }
        catch (Exception e)
        {
            _logger.LogError("Saving image failed due to an exception: {@Excepion}", e);
            OnProgressEvent(new ProgressEventArgs($"download save failed: {args.FileName}"));
        }
    }
}
