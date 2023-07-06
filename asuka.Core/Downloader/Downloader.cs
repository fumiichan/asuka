using asuka.Core.Chaptering.Models;
using asuka.Core.Events;
using asuka.Core.Models;
using asuka.Core.Requests;

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

    public Downloader(IGalleryImageRequestService api, Chapter chapter, string outputPath)
    {
        _api = api;
        _chapter = chapter;
        _output = outputPath;
    }

    public async Task Start()
    {
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
            OnProgressEvent(new ProgressEvent("skip existing"));
            return;
        }

        var image = await provider.FetchImage(args.ImageDownloadPath);
        if (image is null)
        {
            OnProgressEvent(new ProgressEvent("download failed"));
            return;
        }
        
        var imageData = await image.ReadAsByteArrayAsync();

        await File.WriteAllBytesAsync(filePath, imageData)
            .ConfigureAwait(false);

        OnProgressEvent(new ProgressEvent("downloading"));
    }
}
