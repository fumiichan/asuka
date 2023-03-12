using System;
using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Requests;
using asuka.Output;
using asuka.Output.ProgressService;
using asuka.Output.Writer;
using Sharprompt;

namespace asuka.Commandline.Parsers;

public class RandomCommandService : ICommandLineParser
{
    private readonly IDownloader _download;
    private readonly IGalleryRequestService _api;
    private readonly IConsoleWriter _console;
    private readonly IProgressService _progress;
    private readonly IPackArchiveToCbz _pack;

    public RandomCommandService(
        IDownloader download,
        IGalleryRequestService api,
        IConsoleWriter console,
        IProgressService progress,
        IPackArchiveToCbz pack)
    {
        _download = download;
        _api = api;
        _console = console;
        _progress = progress;
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

            _download.CreateSeries(response.Title, opts.Output);
            _download.CreateChapter(response, 1);
            
            _progress.CreateMasterProgress(response.TotalPages, $"downloading random id: {response.Id}");
            var progress = _progress.GetMasterProgress();
            _download.SetOnImageDownload = () =>
            {
                progress.Tick();
            };

            await _download.Start();
            await _download.Final();
            
            if (opts.Pack)
            {
                await _pack.RunAsync(_download.DownloadRoot, opts.Output, progress);
            }
            break;
        }
    }
}
