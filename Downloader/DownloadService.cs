using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ShellProgressBar;
using asuka.Api;
using asuka.Compression;
using asuka.Models;
using asuka.Output;
using asuka.Utils;

namespace asuka.Downloader
{
    public class DownloadService : IDownloadService
    {
        private readonly IGalleryImage _api;
        private readonly IConsoleWriter _console;
        private int _taskId;
        private string _destinationPath;

        public DownloadService(IGalleryImage api, IConsoleWriter console)
        {
            _api = api;
            _console = console;
        }

        public async Task DownloadAsync(GalleryResult result,
            string outputPath,
            bool pack,
            IProgressBar progress = null)
        {
            // Prepare the download.
            await PrepareAsync(result, outputPath);

            // If the progress is null, we create a new one.
            var progressTheme = ProgressBarConfiguration.BarOption;
            var progressInitialTitle = $"[queued] id: {_taskId}";

            using var bar = progress == null
                ? new ProgressBar(result.TotalPages, progressInitialTitle, progressTheme)
                : (IProgressBar) progress.Spawn(result.TotalPages, progressInitialTitle, progressTheme);

            using var throttler = new SemaphoreSlim(2);
            var taskList = new List<Task>();

            foreach (var page in result.Images)
            {
                await throttler.WaitAsync();

                var referenceBar = bar;
                var referenceThrottler = throttler;
                taskList.Add(Task.Run(async () =>
                {
                    await FetchImageAsync(result.MediaId, page, referenceBar);
                    referenceThrottler.Release();
                }));
            }

            await Task.WhenAll(taskList);

            if (pack)
            {
                _console.WriteLine("Compressing...");
                await CompressAsync();
                _console.SuccessLine("Successfully compressed.");
            }
        }

        private async Task CompressAsync()
        {
            var archiveName = $"{_destinationPath}.cbz";
            await ZipExtension.IsArchiveValid(archiveName);

            await Task.Run(() =>
            {
                ZipFile.CreateFromDirectory(_destinationPath, archiveName);
            });
        }

        private async Task PrepareAsync(GalleryResult result, string outputPath)
        {
            // Use the current working directory if the path is empty.
            var destinationPath = Environment.CurrentDirectory;
            if (!string.IsNullOrEmpty(outputPath))
            {
                destinationPath = outputPath;
            }

            var galleryTitle = string.IsNullOrEmpty(result.Title.Japanese)
                ? (string.IsNullOrEmpty(result.Title.English) ? result.Title.Pretty : result.Title.English)
                : result.Title.Japanese;

            // Sanitize the path and the folder name.
            var illegalRegex = new string(Path.GetInvalidFileNameChars()) + ".";
            var regex = new Regex($"[{Regex.Escape(illegalRegex)}]");
            var multiSpacingRegex = new Regex("[ ]{2,}");

            var folderName = regex.Replace($"{result.Id} - {galleryTitle}", "");
            folderName = folderName.Trim();
            folderName = multiSpacingRegex.Replace(folderName, "");

            destinationPath = Path.Join(destinationPath, folderName);

            // Check if the output path exists.
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            // Write the metadata inside the directory.
            var metadataPath = Path.Combine(destinationPath, "info.txt");
            await File.WriteAllTextAsync(metadataPath, result.ToReadable());

            _taskId = result.Id;
            _destinationPath = destinationPath;
        }

        private async Task FetchImageAsync(int mediaId, GalleryImageResult page, IProgressBar bar)
        {
            var image = await _api.GetImage(mediaId.ToString(), page.ServerFilename);
            var imageContents = await image.ReadAsByteArrayAsync();

            var filePath = Path.Combine(_destinationPath, page.Filename);
            await File.WriteAllBytesAsync(filePath, imageContents);

            bar.Tick($"[downloading] id: {_taskId}");
        }
    }
}