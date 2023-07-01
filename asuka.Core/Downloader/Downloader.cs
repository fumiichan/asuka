using asuka.Core.Chaptering;
using asuka.Core.Events;
using asuka.Core.Models;
using asuka.Core.Requests;

namespace asuka.Core.Downloader;

internal class DownloadImageArgs
{
    public string Output { get; init; }
    public string FileName { get; init; }
    public string ImageDownloadPath { get; init; }
}

public sealed class Downloader : IDownloader
{
    private readonly IEnumerable<IGalleryImageRequestService> _apis;
    private EventHandler<ProgressEvent> _progressEvent;

    public Downloader(IEnumerable<IGalleryImageRequestService> apis)
    {
        _apis = apis;
    }

    private void OnProgressEvent(ProgressEvent e)
    {
        _progressEvent?.Invoke(this, e);
    }

    public void HandleOnProgress(Action<object, ProgressEvent> handler)
    {
        _progressEvent += (o, e) =>
        {
            handler(o, e);
        };
    }

    public async Task Start(Chapter chapter)
    {
        var api = _apis
            .FirstOrDefault(x => x.ProviderFor().For == chapter.GetSource());

        if (api is null)
        {
            return;
        }

        await ExecuteTask(chapter, api);
    }

    private async Task ExecuteTask(Chapter chapter, IGalleryImageRequestService api)
    {
        var throttler = new SemaphoreSlim(2);
        var taskList = chapter.GetGalleryResult().Images
            .Select(x => Task.Run(async () =>
            {
                await DownloadTask(chapter, api, throttler, x);
            }));

        await Task.WhenAll(taskList).ConfigureAwait(false);
    }

    private async Task DownloadTask(Chapter chapter,
        IGalleryImageRequestService api,
        SemaphoreSlim throttler,
        GalleryImageResult result)
    {
        await throttler.WaitAsync().ConfigureAwait(false);

        try
        {
            await DownloadImage(api, new DownloadImageArgs
                {
                    Output = chapter.GetOutput(),
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
        var filePath = Path.Combine(args.Output, args.FileName);
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
