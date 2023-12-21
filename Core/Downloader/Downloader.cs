using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using asuka.Configuration;
using asuka.Api;
using asuka.Core.Models;
using asuka.Core.Utilities;
using asuka.Output;

namespace asuka.Core.Downloader;

internal record DownloadTaskDetails
{
    internal GalleryResult Result { get; set; }
    internal string ChapterPath { get; set; }
    internal string ChapterRoot { get; init; }
}

internal record FetchImageParameter
{
    internal string DestinationPath { get; init; }
    internal int MediaId { get; init; }
    internal GalleryImageResult Page { get; init; }
}

public class Downloader : IDownloader
{
    private readonly IGalleryImage _api;
    private readonly IAppConfigManager _appConfigManager;
    private DownloadTaskDetails _details;

    public Downloader(IGalleryImage api, IAppConfigManager appConfigManager)
    {
        _api = api;
        _appConfigManager = appConfigManager;
    }

    private static string GetTitle(GalleryTitleResult result)
    {
        if (!string.IsNullOrEmpty(result.Japanese))
        {
            return result.Japanese;
        }
        if (!string.IsNullOrEmpty(result.English))
        {
            return result.English;
        }
        return !string.IsNullOrEmpty(result.Pretty) ? result.Pretty : "Unknown title";
    }

    private async Task WriteMetadata(string output, GalleryResult result)
    {
        var metaPath = Path.Combine(output, "details.json");
        var serializerOptions = new JsonSerializerOptions { WriteIndented = true };
        var metadata = JsonSerializer
            .Serialize(result.ToTachiyomiMetadata(), serializerOptions);

        await File.WriteAllTextAsync(metaPath, metadata).ConfigureAwait(false);
    }

    public void CreateSeries(GalleryTitleResult title, string outputPath)
    {
        var destination = PathUtils.NormalizeJoin(outputPath, GetTitle(title));
        _details = new DownloadTaskDetails
        {
            ChapterRoot = destination
        };
    }

    public void CreateChapter(GalleryResult result, int chapter)
    {
        _details.ChapterPath = Path.Combine(_details.ChapterRoot, $"ch{chapter}");
        _details.Result = result;
        
        if (!Directory.Exists(_details.ChapterPath))
        {
            Directory.CreateDirectory(_details.ChapterPath!);
        }
    }

    public void CreateChapter(GalleryResult result)
    {
        CreateChapter(result, 1);
    }

    public Action SetOnImageDownload { private get; set; } = () => { };
    public string DownloadRoot => _details.ChapterRoot;

    public async Task Start()
    {
        // Break when necessary.
        if (_details is null)
        {
            return;
        }

        var throttler = new SemaphoreSlim(2);
        var taskList = _details.Result.Images
            .Select(x => Task.Run(async () =>
            {
                await throttler.WaitAsync().ConfigureAwait(false);

                try
                {
                    var param = new FetchImageParameter
                    {
                        DestinationPath = _details.ChapterPath,
                        MediaId = _details.Result.MediaId,
                        Page = x
                    };

                    await DownloadImage(param).ConfigureAwait(false);
                }
                finally
                {
                    throttler.Release();
                }
            }));

        await Task.WhenAll(taskList).ConfigureAwait(false);
    }
    
    public async Task Final()
    {
        await WriteMetadata(_details.ChapterRoot, _details.Result);
    }

    public async Task Final(GalleryResult result)
    {
        await WriteMetadata(_details.ChapterRoot, result);
    }

    private async Task DownloadImage(FetchImageParameter data)
    {
        var filePath = Path.Combine(data.DestinationPath, data.Page.Filename);
        if (File.Exists(filePath))
        {
            SetOnImageDownload();
            return;
        }

        var image = await _api.GetImage(data.MediaId.ToString(), data.Page.ServerFilename);
        var imageData = await image.ReadAsByteArrayAsync();

        await File.WriteAllBytesAsync(filePath, imageData)
            .ConfigureAwait(false);

        SetOnImageDownload();
    }
}
