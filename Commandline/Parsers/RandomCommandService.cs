using System;
using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Requests;
using asuka.Output;
using asuka.Output.Writer;
using Sharprompt;

namespace asuka.Commandline.Parsers;

public class RandomCommandService : ICommandLineParser
{
    private readonly IDownloadService _download;
    private readonly IGalleryRequestService _api;
    private readonly IConsoleWriter _console;
    private readonly IPackArchiveToCbz _pack;

    public RandomCommandService(
        IDownloadService download,
        IGalleryRequestService api,
        IConsoleWriter console,
        IPackArchiveToCbz pack)
    {
        _download = download;
        _api = api;
        _console = console;
        _pack = pack;
    }

    public async Task RunAsync(object options)
    {
        var opts = (RandomOptions)options;
        var totalNumbers = await _api.GetTotalGalleryCountAsync();

        while (true)
        {
            var randomCode = new Random().Next(1, totalNumbers);
            var response = await _api.FetchSingleAsync(randomCode.ToString());

            _console.WriteLine(response.ToReadable());

            var prompt = Prompt.Confirm("Are you sure to download this one?", true);
            if (!prompt)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                continue;
            }

            var result = await _download.DownloadAsync(response, opts.Output);
            if (opts.Pack)
            {
                await _pack.RunAsync(result.DestinationPath, opts.Output, result.ProgressBar);
            }
            break;
        }
    }
}
