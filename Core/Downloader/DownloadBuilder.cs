using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using asuka.Api;
using asuka.Core.Models;

namespace asuka.Core.Downloader;

public class DownloadStatus
{
    public bool Success { get; set; }
    public required string FileName { get; set; }
}

public class DownloadBuilder
{
    private readonly GalleryResult _chapter;
    private readonly int _chapterId;

    public required IGalleryImage Request { get; init; }
    public required string Output { get; init; }
    public Action<DownloadStatus>? OnEachComplete { get; init; }
    public Func<GalleryResult, Task>? OnComplete { get; init; }

    public DownloadBuilder(GalleryResult chapter, int chapterId)
    {
        _chapter = chapter;
        _chapterId = chapterId;
    }

    public async Task Start()
    {
        var chapterPath = Path.Combine(Output, $"ch{_chapterId}");
        if (!Directory.Exists(chapterPath))
        {
            Directory.CreateDirectory(chapterPath);
        }

        var semaphore = new SemaphoreSlim(2);
        var tasks = _chapter.Images
            .Select(x => Task.Run(async () =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                
                var filePath = Path.Combine(chapterPath, x.Filename);
                await DownloadImage(filePath, _chapter.MediaId.ToString(), x.ServerFilename);

                semaphore.Release();
            }));

        await Task.WhenAll(tasks)
            .ConfigureAwait(false);

        OnComplete?.Invoke(_chapter);
    }

    private async Task DownloadImage(string outputPath, string mediaId, string serverFileName)
    {
        var status = new DownloadStatus
        {
            Success = true,
            FileName = Path.GetFileName(outputPath)
        };

        try
        {
            if (File.Exists(outputPath))
            {
                OnEachComplete?.Invoke(status);
                return;
            }

            var image = await Request.GetImage(mediaId, serverFileName);
            var data = await image.ReadAsByteArrayAsync();

            await File.WriteAllBytesAsync(outputPath, data)
                .ConfigureAwait(false);
            
            OnEachComplete?.Invoke(status);
        }
        catch
        {
            status.Success = false;
            OnEachComplete?.Invoke(status);
        }
    }
}
