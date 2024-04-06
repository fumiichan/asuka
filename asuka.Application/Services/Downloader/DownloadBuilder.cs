using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using asuka.Application.Compression;
using asuka.Application.Extensions;
using asuka.Application.Utilities;
using asuka.ProviderSdk;
using Microsoft.Extensions.Logging;
using ShellProgressBar;

namespace asuka.Application.Services.Downloader;

internal sealed class DownloadBuilder : IDownloaderBuilder
{
    private readonly ILogger<DownloadBuilder> _logger;

    public DownloadBuilder(ILogger<DownloadBuilder> logger)
    {
        _logger = logger;
    }
    
    #region Downloader Main Logic

    internal sealed class Downloader
    {
        private readonly List<(MetaInfo, Series)> _queue;
        private readonly ILogger<DownloadBuilder> _logger;

        private readonly DownloaderConfiguration _config = new()
        {
            OutputPath = Environment.CurrentDirectory,
            SaveMetadata = true,
            Pack = false
        };

        internal Downloader(ILogger<DownloadBuilder> logger)
        {
            _logger = logger;
            _queue = [];
        }

        public void AddSeries(MetaInfo client, Series series)
        {
            _queue.Add((client, series));
        }

        public void AddSeriesRange(List<(MetaInfo, Series)> list)
        {
            _queue.AddRange(list);
        }

        public void Configure(Action<DownloaderConfiguration> configDelegate)
        {
            configDelegate(_config);
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            using var progress = new ProgressBar(_queue.Count, "Downloading queue...");
            foreach (var (client, series) in _queue)
            {
                var seriesPath = PathUtils.Join(_config.OutputPath, series.Title);
                
                _logger.LogInformation("Downloading series: {title}", series.Title);
                await ProcessChapters(client, series.Chapters, seriesPath, progress, cancellationToken);

                if (_config.SaveMetadata)
                {
                    var metaPath = Path.Combine(seriesPath, "details.json");
                    await series.WriteMetadata(metaPath);
                }

                if (_config.Pack)
                {
                    await Compress.ToCbz(seriesPath);
                }

                progress.Tick();
                _logger.LogInformation("Download completed for {title}", series.Title);
            }
        }

        private async Task ProcessChapters(MetaInfo client, List<Chapter> chapters, string outputPath,
            ProgressBar bar, CancellationToken cancellationToken = default)
        {
            using var progress = bar.Spawn(chapters.Count, "Downloading chapters");
            foreach (var chapter in chapters)
            {
                _logger.LogInformation("Downloading chapter id: {chapter}", chapter.Id);
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var chapterPath = PathUtils.Join(outputPath, $"ch{chapter.Id}");
                if (!Directory.Exists(chapterPath))
                {
                    Directory.CreateDirectory(chapterPath);
                }

                var semaphore = new SemaphoreSlim(2);

                var pageProgress = progress.Spawn(chapter.Pages.Count, $"Downloading chapter {chapter.Id}");
                var tasks = chapter.Pages
                    .Select(page => Task.Run(async () =>
                    {
                        await semaphore.WaitAsync(cancellationToken)
                            .ConfigureAwait(false);

                        try
                        {
                            var filePath = Path.Combine(chapterPath, page.Filename);
                            if (File.Exists(filePath))
                            {
                                _logger.LogInformation("Download skipped due to file exists: {path}", filePath);
                                return;
                            }

                            var data = await client.GetImage(page.ImageRemotePath, CancellationToken.None);
                            await File.WriteAllBytesAsync(filePath, data, CancellationToken.None);

                            _logger.LogInformation("File downloaded: {file} with {length} bytes", filePath,
                                data.Length);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Download failed due to an exception: {ex}", ex);
                        }
                        finally
                        {
                            pageProgress.Tick();
                            semaphore.Release();
                        }
                    }, cancellationToken));

                await Task.WhenAll(tasks).ConfigureAwait(false);
                
                progress.Tick();
                _logger.LogInformation("Download of chapter completed: {chapter}", chapter.Id);
            }
        }
    }

    #endregion

    public Downloader CreateDownloaderInstance()
    {
        return new Downloader(_logger);
    }
}
