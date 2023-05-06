using System.Linq;
using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Core.Chaptering;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Requests;
using asuka.Output;
using asuka.Output.ProgressService;
using asuka.Output.Writer;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class GetCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IValidator<GetOptions> _validator;
    private readonly IDownloader _download;
    private readonly IConsoleWriter _console;
    private readonly IProgressService _progress;
    private readonly IPackArchiveToCbz _pack;
    private readonly ISeriesFactory _series;

    public GetCommandService(
        IGalleryRequestService api,
        IValidator<GetOptions> validator,
        IDownloader download,
        IConsoleWriter console,
        IProgressService progress,
        IPackArchiveToCbz pack,
        ISeriesFactory series)
    {
        _api = api;
        _validator = validator;
        _download = download;
        _console = console;
        _progress = progress;
        _pack = pack;
        _series = series;
    }

    private async Task DownloadTask(int input, bool pack, bool readOnly, string outputPath)
    {
        var response = await _api.FetchSingleAsync(input.ToString());
        _console.WriteLine(response.ToReadable());

        // Don't download.
        if (readOnly)
        {
            return;
        }
        
        _series.AddChapter(response, outputPath);

        _progress.CreateMasterProgress(response.TotalPages, $"downloading: {response.Id}");
        var progress = _progress.GetMasterProgress();

        _download.HandleOnDownloadComplete((_, e) =>
        {
            progress.Tick($"{e.Message}: {response.Id}");
        });

        await _download.Start(_series.GetSeries().Chapters.First());
        await _series.Close();

        if (pack)
        {
            await _pack.RunAsync(_series.GetSeries().Output, outputPath, progress);
        }
        
        _series.Reset();
    }

    public async Task RunAsync(object options)
    {
        var opts = (GetOptions)options;
        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            _console.ValidationErrors(validationResult.Errors);
            return;
        }

        foreach (var code in opts.Input)
        {
            await DownloadTask(code, opts.Pack, opts.ReadOnly, opts.Output);
        }
    }
}
