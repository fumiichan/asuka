using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using asuka.Configuration;
using asuka.Core.Api;
using asuka.Core.Models;
using asuka.Core.Utilities;
using asuka.Output;
using Newtonsoft.Json;

namespace asuka.Core.Downloader;

internal record DownloadTaskDetails
{
    internal GalleryResult Result { get; init; }
    internal string ChapterPath { get; init; }
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
    private readonly IConfigurationManager _configurationManager;
    private DownloadTaskDetails _details;

    public Downloader(IGalleryImage api, IConfigurationManager configurationManager)
    {
        _api = api;
        _configurationManager = configurationManager;
    }

    private static string GetTitle(GalleryTitleResult result)
    {
        if (!string.IsNullOrEmpty(result.Japanese)) return result.Japanese;
        if (!string.IsNullOrEmpty(result.English)) return result.English;
        if (!string.IsNullOrEmpty(result.Pretty)) return result.Pretty;

        return "Unknown title";
    }

    private async Task WriteMetadata(string output, GalleryResult result, int chapter = 0)
    {
        if (_configurationManager.Values.UseTachiyomiLayout && chapter != -1)
        {
            var metaPath = Path.Combine(output, "details.json");
            var metadata = JsonConvert
                .SerializeObject(result.ToTachiyomiMetadata(), Formatting.Indented);

            await File.WriteAllTextAsync(metaPath, metadata).ConfigureAwait(false);
            return;
        }

        var metadataPath = Path.Combine(output, "info.txt");
        await File.WriteAllTextAsync(metadataPath, result.ToReadable())
            .ConfigureAwait(false);
    }

    public async Task Initialize(GalleryResult result, string outputPath, int chapter = -1)
    {
        // Override given chapter when using the layout is false.
        var chapterNumber = _configurationManager.Values.UseTachiyomiLayout ? chapter : -1;
        var destination = PathUtils.NormalizeJoin(outputPath, GetTitle(result.Title), chapterNumber);
        if (!Directory.Exists(destination.ChapterPath))
        {
            Directory.CreateDirectory(destination.ChapterPath!);
        }

        await WriteMetadata(destination.ChapterRoot, result, chapter);

        _details = new DownloadTaskDetails
        {
            ChapterPath = destination.ChapterPath,
            ChapterRoot = destination.ChapterRoot,
            Result = result
        };
    }

    public Action OnImageDownload { get; set; } = () => { };
    public string DownloadRoot => _details.ChapterRoot;

    public async Task Start()
    {
        // Break when necessary.
        if (_details is null)
        {
            throw new NullReferenceException("Data required for download task is missing.");
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

    private async Task DownloadImage(FetchImageParameter data)
    {
        var filePath = Path.Combine(data.DestinationPath, data.Page.Filename);
        if (File.Exists(filePath))
        {
            OnImageDownload();
            return;
        }

        var image = await _api.GetImage(data.MediaId.ToString(), data.Page.ServerFilename);
        var imageData = await image.ReadAsByteArrayAsync();

        await File.WriteAllBytesAsync(filePath, imageData)
            .ConfigureAwait(false);

        OnImageDownload();
    }
}
