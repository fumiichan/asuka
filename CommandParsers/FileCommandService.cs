using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using asuka.CommandOptions;
using asuka.Configuration;
using asuka.Downloader;
using asuka.Output;
using asuka.Services;
using asuka.Utils;
using Microsoft.Extensions.Configuration;
using ShellProgressBar;

namespace asuka.CommandParsers;

public class FileCommandService : IFileCommandService
{
    private readonly IGalleryRequestService _api;
    private readonly IConsoleWriter _console;
    private readonly IDownloadService _download;
    private readonly IConfigurationManager _configurationManager;

    public FileCommandService(IGalleryRequestService api, IConsoleWriter console, IDownloadService download, IConfigurationManager configurationManager)
    {
        _api = api;
        _console = console;
        _download = download;
        _configurationManager = configurationManager;
    }

    public async Task RunAsync(FileCommandOptions opts)
    {
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

        using var progress = new ProgressBar(
            validUrls.Count,
            "downloading from text file...",
            ProgressBarConfiguration.BarOption);

        var useTachiyomiLayout = opts.UseTachiyomiLayout || _configurationManager.Values.UseTachiyomiLayout;

        foreach (var url in validUrls)
        {
            var code = Regex.Match(url, @"\d+").Value;
            var response = await _api.FetchSingleAsync(code);

            await _download.DownloadAsync(response, opts.Output, opts.Pack, useTachiyomiLayout, progress);
            progress.Tick();
        }
    }

    private static IReadOnlyList<string> FilterValidUrls(IEnumerable<string> urls)
    {
        return urls.Where(url =>
        {
            const string pattern = @"^http(s)?:\/\/(nhentai\.net)\b([//g]*)\b([\d]{1,6})\/?$";
            var regexp = new Regex(pattern, RegexOptions.IgnoreCase);

            return regexp.IsMatch(url);
        }).ToList();
    }

    private static bool IsFileExceedingToFileSizeLimit(string inputFile)
    {
        var fileSize = new FileInfo(inputFile).Length;
        return fileSize > 5242880;
    }
}
