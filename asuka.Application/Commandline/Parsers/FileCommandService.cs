using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Output.Writer;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Core.Output.Progress;
using asuka.Core.Requests;

namespace asuka.Application.Commandline.Parsers;

public class FileCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IConsoleWriter _console;
    private readonly IDownloader _download;
    private readonly IProgressService _progressService;
    private readonly ISeriesFactory _series;

    public FileCommandService(
        IGalleryRequestService api,
        IConsoleWriter console,
        IDownloader download,
        IProgressService progressService,
        ISeriesFactory series)
    {
        _api = api;
        _console = console;
        _download = download;
        _progressService = progressService;
        _series = series;
    }

    public async Task RunAsync(object options)
    {
        var opts = (FileCommandOptions)options;
        if (!File.Exists(opts.FilePath))
        {
            _console.ErrorLine("File doesn't exist.");
            return;
        }

        if (IsFileExceedingToFileSizeLimit(opts.FilePath))
        {
            _console.ErrorLine("The file size is exceeding 5MB file size limit.");
            return;
        }

        var textFile = await File.ReadAllLinesAsync(opts.FilePath, Encoding.UTF8)
            .ConfigureAwait(false);
        var validUrls = FilterValidUrls(textFile);

        if (validUrls.Count == 0)
        {
            _console.ErrorLine("No valid URLs found.");
            return;
        }

        _progressService.CreateMasterProgress(validUrls.Count, "downloading from text file...");
        var progress = _progressService.GetMasterProgress();

        foreach (var url in validUrls)
        {
            var code = new Regex("\\d+").Match(url).Value;
            var response = await _api.FetchSingleAsync(code);

            _series.AddChapter(response, opts.Output);
            
            // Create progress bar
            var internalProgress = _progressService.NestToMaster(response.TotalPages, $"downloading: {response.Id}");
            _download.HandleOnProgress((_, e) =>
            {
                internalProgress.Tick($"{e.Message}: {response.Id}");
            });

            // Start downloading
            await _download.Start(_series.GetSeries().Chapters.First());
            await _series.Close(opts.Pack ? internalProgress : null);
            
            progress.Tick();
        }
    }

    private static IReadOnlyList<string> FilterValidUrls(IEnumerable<string> urls)
    {
        return urls.Where(url => new Regex("^http(s)?:\\/\\/(nhentai\\.net)\\b([//g]*)\\b([\\d]{1,6})\\/?$").IsMatch(url)).ToList();
    }

    private static bool IsFileExceedingToFileSizeLimit(string inputFile)
    {
        var fileSize = new FileInfo(inputFile).Length;
        return fileSize > 5242880;
    }
}
