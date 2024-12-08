using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using asuka.Application.Extensions;
using asuka.Application.Services.Downloader.Compression;
using asuka.Application.Utilities;
using asuka.ProviderSdk;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace asuka.Application.Services.Downloader;

internal sealed class DownloadBuilder : IDownloaderBuilder
{
    private readonly ILogger<DownloadBuilder> _logger;

    public DownloadBuilder(ILogger<DownloadBuilder> logger)
    {
        _logger = logger;
    }

    public Downloader CreateDownloaderInstance(MetaInfo client, Series series)
    {
        return new Downloader(_logger, client, series);
    }
}

internal sealed class Downloader
{
    private readonly DownloaderConfiguration _config = new()
    {
        OutputPath = Environment.CurrentDirectory,
        SaveMetadata = true,
        Pack = false
    };

    private readonly ILogger<DownloadBuilder> _logger;
    
    // Events
    public Action<string> OnProgress = (_) => { };

    private readonly MetaInfo _client;
    private readonly Series _series;

    internal Downloader(ILogger<DownloadBuilder> logger, MetaInfo client, Series series)
    {
        _logger = logger;
        _client = client;
        _series = series;
    }

    public void Configure(Action<DownloaderConfiguration> configDelegate)
    {
        configDelegate(_config);
    }

    public async Task Start(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading series: {series}", _series);
        
        var seriesPath = PathUtils.Join(_config.OutputPath, _series.Title);
        foreach (var chapter in _series.Chapters)
        {
            OnProgress.Invoke($"Downloading {_series.Title} (Chapter {chapter.Id} of {_series.Chapters.Count}) (0/{chapter.Pages.Count})...");
            if (cancellationToken.IsCancellationRequested)
            {
                AnsiConsole.MarkupLine("[yellow]Cancelled.[/]");
                break;
            }

            await ProcessChapter(chapter, seriesPath, cancellationToken);
            AnsiConsole.MarkupLine("[chartreuse1]Finished: {0} (Chapter {1} of {2})[/]", Markup.Escape(_series.Title), chapter.Id, _series.Chapters.Count);
        }

        if (_config.SaveMetadata)
        {
            var metaPath = Path.Combine(seriesPath, "details.json");
            await _series.WriteMetadata(metaPath);
        }

        if (_config.Pack)
        {
            OnProgress.Invoke($"Compressing: {_series.Title}...");
            await Compress.ToCbz(seriesPath);
        }
    }

    private async Task ProcessChapter(Chapter chapter, string outputPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading chapter id: {chapter}", chapter.Id);

        var chapterPath = PathUtils.Join(outputPath, $"ch{chapter.Id}");
        if (!Directory.Exists(chapterPath)) Directory.CreateDirectory(chapterPath);

        var tick = 0;
        foreach (var page in chapter.Pages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var filePath = Path.Combine(chapterPath, page.Filename);
                if (File.Exists(filePath))
                {
                    _logger.LogInformation("Download skipped due to file exists: {path}", filePath);
                    continue;
                }

                var data = await _client.GetImage(page.ImageRemotePath, CancellationToken.None);
                await File.WriteAllBytesAsync(filePath, data, CancellationToken.None);

                _logger.LogInformation("File downloaded: {file} with {length} bytes", filePath,
                    data.Length);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red3_1]Failed to download image: {0}[/]", page.ImageRemotePath);
                _logger.LogError("Download failed due to an exception: {ex}", ex);
            }
            finally
            {
                tick++;
                OnProgress.Invoke($"Downloading {_series.Title} (Chapter 1 of {_series.Chapters.Count}) ({tick}/{chapter.Pages.Count})...");
            }
        }
        _logger.LogInformation("Download of chapter completed: {chapter}", chapter.Id);
    }
}