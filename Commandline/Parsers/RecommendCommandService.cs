using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Mappings;
using asuka.Core.Requests;
using asuka.Output.ProgressService;
using asuka.Output.Writer;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class RecommendCommandService : IRecommendCommandService
{
    private readonly IValidator<IRequiresInputOption> _validator;
    private readonly IGalleryRequestService _api;
    private readonly IDownloadService _download;
    private readonly IConsoleWriter _console;
    private readonly IProgressService _progressService;
    private readonly IPackArchiveToCbz _pack;

    public RecommendCommandService(
        IValidator<IRequiresInputOption> validator,
        IGalleryRequestService api,
        IDownloadService download,
        IConsoleWriter console,
        IProgressService progressService,
        IPackArchiveToCbz pack)
    {
        _validator = validator;
        _api = api;
        _download = download;
        _console = console;
        _progressService = progressService;
        _pack = pack;
    }

    public async Task RunAsync(RecommendOptions opts)
    {
        var validator = await _validator.ValidateAsync(opts);
        if (!validator.IsValid)
        {
            _console.ValidationErrors(validator.Errors);
            return;
        }

        var responses = await _api.FetchRecommendedAsync(opts.Input.ToString());
        var selection = responses.FilterByUserSelected();

        // Initialise the Progress bar.
        _progressService.CreateMasterProgress(selection.Count, $"[task] recommend from id: {opts.Input}");
        var progress = _progressService.GetMasterProgress();

        foreach (var response in selection)
        {
            var result = await _download.DownloadAsync(response, opts.Output);
            if (opts.Pack)
            {
                var destination = result.DestinationPath[..^1] + ".cbz";
                await _pack.RunAsync(result.FolderName, result.ImageFiles, destination);
            }
            progress.Tick();
        }
    }
}
