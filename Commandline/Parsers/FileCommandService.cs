using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Requests;
using asuka.Output.ProgressService;
using asuka.Output.Writer;

namespace asuka.Commandline.Parsers;

public partial class FileCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IConsoleWriter _console;
    private readonly IDownloader _download;
    private readonly IProgressService _progressService;
    private readonly IPackArchiveToCbz _pack;

    public FileCommandService(
        IGalleryRequestService api,
        IConsoleWriter console,
        IDownloader download,
        IProgressService progressService,
        IPackArchiveToCbz pack)
    {
        _api = api;
        _console = console;
        _download = download;
        _progressService = progressService;
        _pack = pack;
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
            var code = NumericRegex().Match(url).Value;
            var response = await _api.FetchSingleAsync(code);

            await _download.Initialize(response, opts.Output, 1);
            
            // Create progress bar
            var internalProgress = _progressService.NestToMaster(response.TotalPages, $"downloading: {response.Id}");
            _download.OnImageDownload = () =>
            {
                internalProgress.Tick($"downloading: {response.Id}");
            };

            // Start downloading
            await _download.Start();
            
            // If --pack option is specified, compresss the file into cbz
            if (opts.Pack)
            {
                await _pack.RunAsync(_download.DownloadRoot, opts.Output, internalProgress);
            }
            progress.Tick();
        }
    }

    private static IReadOnlyList<string> FilterValidUrls(IEnumerable<string> urls)
    {
        return urls.Where(url => WebUrlRegex().IsMatch(url)).ToList();
    }

    private static bool IsFileExceedingToFileSizeLimit(string inputFile)
    {
        var fileSize = new FileInfo(inputFile).Length;
        return fileSize > 5242880;
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex NumericRegex();

    [GeneratedRegex("^http(s)?:\\/\\/(nhentai\\.net)\\b([//g]*)\\b([\\d]{1,6})\\/?$", RegexOptions.IgnoreCase, "en-JP")]
    private static partial Regex WebUrlRegex();
}
