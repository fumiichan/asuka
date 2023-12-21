using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using asuka.Api;
using asuka.Commandline.Options;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Extensions;
using asuka.Core.Requests;
using asuka.Core.Utilities;
using asuka.Output.Progress;

namespace asuka.Commandline.Parsers;

public class FileCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IGalleryImage _apiImage;
    private readonly IProgressFactory _progress;

    public FileCommandService(
        IGalleryRequestService api,
        IGalleryImage apiImage,
        IProgressFactory progress)
    {
        _api = api;
        _apiImage = apiImage;
        _progress = progress;
    }

    public async Task RunAsync(object options)
    {
        var opts = (FileCommandOptions)options;
        if (!File.Exists(opts.FilePath))
        {
            Console.WriteLine("File doesn't exist.");
            return;
        }

        if (IsFileExceedingToFileSizeLimit(opts.FilePath))
        {
            Console.WriteLine("The file size is exceeding 5MB file size limit.");
            return;
        }

        var textFile = await File.ReadAllLinesAsync(opts.FilePath, Encoding.UTF8)
            .ConfigureAwait(false);
        var validUrls = FilterValidUrls(textFile);

        if (validUrls.Count == 0)
        {
            Console.WriteLine("No valid URLs found.");
            return;
        }

        var mainProgress = _progress.Create(validUrls.Count,
            $"Downloading from text file...");

        foreach (var url in validUrls)
        {
            var code = new Regex("\\d+").Match(url).Value;
            var response = await _api.FetchSingleAsync(code);

            var childProgress = mainProgress.Spawn(response.TotalPages,
                $"Downloading: {response.Title.GetTitle()}")!;

            var output = Path.Combine(opts.Output, PathUtils.NormalizeName(response.Title.GetTitle()));
            var downloader = new DownloadBuilder(response, 1)
            {
                Request = _apiImage,
                Output = output,
                OnEachComplete = _ =>
                {
                    childProgress.Tick();
                },
                OnComplete = async gallery =>
                {
                    await gallery.WriteMetadata(Path.Combine(output, "details.json"));
                    if (opts.Pack)
                    {
                        await Compress.ToCbz(output, childProgress);
                    }
                }
            };

            await downloader.Start();
            mainProgress.Tick();
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
