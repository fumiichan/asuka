using System.Threading.Tasks;
using asuka.Commandline.Options;
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
    private readonly IValidator<IRequiresInputOption> _validator;
    private readonly IDownloader _download;
    private readonly IConsoleWriter _console;
    private readonly IProgressService _progress;
    private readonly IPackArchiveToCbz _pack;

    public GetCommandService(
        IGalleryRequestService api,
        IValidator<IRequiresInputOption> validator,
        IDownloader download,
        IConsoleWriter console,
        IProgressService progress,
        IPackArchiveToCbz pack)
    {
        _api = api;
        _validator = validator;
        _download = download;
        _console = console;
        _progress = progress;
        _pack = pack;
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

        var response = await _api.FetchSingleAsync(opts.Input.ToString());
        _console.WriteLine(response.ToReadable());

        if (opts.ReadOnly)
        {
            return;
        }

        _download.CreateSeries(response.Title, opts.Output);
        _download.CreateChapter(response, 1);

        _progress.CreateMasterProgress(response.TotalPages, $"downloading: {response.Id}");

        var progress = _progress.GetMasterProgress();
        _download.OnImageDownload = () =>
        {
            progress.Tick();
        };

        await _download.Start();
        await _download.Finalize();

        if (opts.Pack)
        {
            await _pack.RunAsync(_download.DownloadRoot, opts.Output, progress);
        }
    }
}
