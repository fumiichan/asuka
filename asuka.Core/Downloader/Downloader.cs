using asuka.Core.Chaptering;
using asuka.Core.Events;
using asuka.Core.Requests;

namespace asuka.Core.Downloader;

internal class DownloadImageArgs
{
    public string Output { get; init; }
    public string FileName { get; set; }
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

    public async Task Start(string providerName, Chapter chapter)
    {
        var api = _apis
            .FirstOrDefault(x => x.ProviderFor() == providerName);

        if (api is null)
        {
            return;
        }

        await ExecuteTask(chapter, api);
    }

    private async Task ExecuteTask(Chapter chapter, IGalleryImageRequestService api)
    {
        var throttler = new SemaphoreSlim(2);
        var taskList = chapter.Data.Images
            .Select(x => Task.Run(async () =>
            {
                await throttler.WaitAsync().ConfigureAwait(false);

                try
                {
                    await DownloadImage(api, new DownloadImageArgs
                        {
                            Output = chapter.Output,
                            FileName = x.Filename,
                            ImageDownloadPath = x.ServerFilename
                        })
                        .ConfigureAwait(false);
                }
                finally
                {
                    throttler.Release();
                }
            }));

        await Task.WhenAll(taskList).ConfigureAwait(false);
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
