using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Output;
using asuka.Application.Output.Writer;
using asuka.Core.Chaptering;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Output.Progress;
using asuka.Core.Requests;
using FluentValidation;

namespace asuka.Application.Commandline.Parsers;

public class GetCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IValidator<GetOptions> _validator;
    private readonly IDownloader _download;
    private readonly IConsoleWriter _console;
    private readonly IProgressService _progress;
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

        _download.HandleOnProgress((_, e) =>
        {
            progress.Tick($"{e.Message}: {response.Id}");
        });

        await _download.Start(_series.GetSeries().Chapters.First());
        await _series.Close(pack ? progress : null);
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
