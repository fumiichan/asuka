using asuka.Api;
using asuka.Core.Chaptering;
using asuka.Core.Events;
using asuka.Core.Models;

namespace asuka.Core.Downloader;

public sealed class Downloader : IDownloader
{
    private readonly IGalleryImage _api;
    private EventHandler<ProgressEvent> _progressEvent;

    public Downloader(IGalleryImage api)
    {
        _api = api;
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
        var throttler = new SemaphoreSlim(2);
        var taskList = chapter.Data.Images
            .Select(x => Task.Run(async () =>
            {
                await throttler.WaitAsync().ConfigureAwait(false);

                try
                {
                    await DownloadImage(chapter.Output, chapter.Data.MediaId, x)
                        .ConfigureAwait(false);
                }
                finally
                {
                    throttler.Release();
                }
            }));

        await Task.WhenAll(taskList).ConfigureAwait(false);
    }

    private async Task DownloadImage(string output, int mediaId, GalleryImageResult page)
    {
        var filePath = Path.Combine(output, page.Filename);
        if (File.Exists(filePath))
        {
            OnProgressEvent(new ProgressEvent("skip existing"));
            return;
        }

        var image = await _api.GetImage(mediaId.ToString(), page.ServerFilename);
        var imageData = await image.ReadAsByteArrayAsync();

        await File.WriteAllBytesAsync(filePath, imageData)
            .ConfigureAwait(false);

        OnProgressEvent(new ProgressEvent("downloading"));
    }
}
