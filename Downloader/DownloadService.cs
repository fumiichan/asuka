using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly IPackArchiveToCbz _packer;
        private int _taskId;
        private string _destinationPath;
        private string _folderName;

        public DownloadService(IGalleryImage api, IPackArchiveToCbz packer)
        {
            _api = api;
            _packer = packer;
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
                var imageFiles = Directory.GetFiles(_destinationPath);
                await _packer.RunAsync(_folderName, imageFiles, $"{_destinationPath}.cbz", bar);
            }
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

        private async Task PrepareAsync(GalleryResult result, string outputPath)
        {
            _taskId = result.Id;
            _destinationPath = Environment.CurrentDirectory;
            if (!string.IsNullOrEmpty(outputPath))
            {
                _destinationPath = outputPath;
            }

            var galleryTitle = string.IsNullOrEmpty(result.Title.Japanese)
                ? (string.IsNullOrEmpty(result.Title.English) ? result.Title.Pretty : result.Title.English)
                : result.Title.Japanese;

            var folderName = SantizeFolderName($"{result.Id} - {galleryTitle}");
            _folderName = folderName;

            _destinationPath = Path.Join(_destinationPath, folderName);
            if (!Directory.Exists(_destinationPath))
            {
                Directory.CreateDirectory(_destinationPath);
            }

            var metadataPath = Path.Combine(_destinationPath, "info.txt");
            await File.WriteAllTextAsync(metadataPath, result.ToReadable());
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
