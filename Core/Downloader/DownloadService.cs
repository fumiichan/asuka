using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using asuka.Configuration;
using asuka.Core.Api;
using asuka.Core.Downloader.InternalTypes;
using asuka.Core.Models;
using asuka.Output;
using asuka.Output.ProgressService;
using Newtonsoft.Json;
using ShellProgressBar;

namespace asuka.Core.Downloader;

public class DownloadService : IDownloadService
{
    private readonly IGalleryImage _api;
    private readonly IProgressService _progress;
    private readonly IConfigurationManager _configurationManager;

    public DownloadService(IGalleryImage api, IProgressService progress, IConfigurationManager configurationManager)
    {
        _api = api;
        _progress = progress;
        _configurationManager = configurationManager;
    }

    public async Task<DownloadResult> DownloadAsync(GalleryResult result, string outputPath)
    {
        var useTachiyomiFolderLayout = _configurationManager.Values.UseTachiyomiLayout;
        // Prepare the download.
        var prepare = await PrepareAsync(result, outputPath, useTachiyomiFolderLayout).ConfigureAwait(false);

        // If the progress is null, we create a new one.
        var progressInitialTitle = $"[queued] id: {prepare.Id}";
        var bar = _progress.NestToMaster(result.TotalPages, progressInitialTitle);

        var throttler = new SemaphoreSlim(2);
        var taskList = result.Images
            .Select(x => Task.Run(async () =>
            {
                await throttler.WaitAsync().ConfigureAwait(false);

                try
                {
                    var param = new FetchImageParameter
                    {
                        DestinationPath = prepare.DestinationPath,
                        MediaId = result.MediaId,
                        Page = x,
                        TaskId = prepare.Id
                    };

                    await FetchImageAsync(param, bar).ConfigureAwait(false);
                }
                finally
                {
                    throttler.Release();
                }
            }));

        await Task.WhenAll(taskList).ConfigureAwait(false);

        var destination = useTachiyomiFolderLayout
            ? Path.GetFullPath($"{prepare.DestinationPath}/../") : prepare.DestinationPath;
        return new DownloadResult
        {
            DestinationPath = destination,
            ProgressBar = bar
        };
    }

    private static string SantizeFolderName(string folderName)
    {
        var illegalRegex = new string(Path.GetInvalidFileNameChars()) + ".";
        var regex = new Regex($"[{Regex.Escape(illegalRegex)}]");
        var multiSpacingRegex = new Regex("[ ]{2,}");

        folderName = regex.Replace(folderName, "");
        folderName = folderName.Trim();
        folderName = multiSpacingRegex.Replace(folderName, "");

        return folderName;
    }

    private async Task<PrepareResult> PrepareAsync(GalleryResult result, string outputPath, bool useTachiyomiFolderLayout)
    {
        var destinationPath = Environment.CurrentDirectory;
        if (!string.IsNullOrEmpty(outputPath))
        {
            destinationPath = outputPath;
        }

        var galleryTitle = string.IsNullOrEmpty(result.Title.Japanese)
            ? (string.IsNullOrEmpty(result.Title.English) ? result.Title.Pretty : result.Title.English)
            : result.Title.Japanese;

        var folderName = SantizeFolderName($"{result.Id} - {galleryTitle}");

        var mangaRootPath = Path.Join(destinationPath, folderName);
        destinationPath = useTachiyomiFolderLayout ? Path.Join(mangaRootPath, "ch1") : mangaRootPath;
        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }

        var metadataPath = Path.Combine(mangaRootPath, "info.txt");
        await File.WriteAllTextAsync(metadataPath, result.ToReadable())
            .ConfigureAwait(false);

        // Generate Tachiyomi details.json
        if (useTachiyomiFolderLayout)
        {
            var tachiyomiMetadataPath = Path.Combine(mangaRootPath, "details.json");
            var tachiyomiMetadata = JsonConvert
                .SerializeObject(result.ToTachiyomiMetadata(), Formatting.Indented);
            await File.WriteAllTextAsync(tachiyomiMetadataPath, tachiyomiMetadata)
                .ConfigureAwait(false);
        }

        return new PrepareResult
        {
            FolderName = folderName,
            Id = result.Id,
            DestinationPath = destinationPath
        };
    }

    private async Task FetchImageAsync(FetchImageParameter data, IProgressBar bar)
    {
        var filePath = Path.Combine(data.DestinationPath, data.Page.Filename);
        if (File.Exists(filePath))
        {
            bar.Tick($"[skipped existing] id: {data.TaskId}");
            return;
        }

        var image = await _api.GetImage(data.MediaId.ToString(), data.Page.ServerFilename);
        var imageContents = await image.ReadAsByteArrayAsync();
        
        await File.WriteAllBytesAsync(filePath, imageContents)
            .ConfigureAwait(false);

        bar.Tick($"[downloading] id: {data.TaskId}");
    }
}
