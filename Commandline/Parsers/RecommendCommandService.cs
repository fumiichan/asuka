using System.Linq;
using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Core.Chaptering;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Mappings;
using asuka.Core.Requests;
using asuka.Output.ProgressService;
using asuka.Output.Writer;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class RecommendCommandService : ICommandLineParser
{
    private readonly IValidator<RecommendOptions> _validator;
    private readonly IGalleryRequestService _api;
    private readonly IDownloader _download;
    private readonly IConsoleWriter _console;
    private readonly IProgressService _progressService;
    private readonly IPackArchiveToCbz _pack;
    private readonly ISeriesFactory _series;

    public RecommendCommandService(
        IValidator<RecommendOptions> validator,
        IGalleryRequestService api,
        IDownloader download,
        IConsoleWriter console,
        IProgressService progressService,
        IPackArchiveToCbz pack,
        ISeriesFactory series)
    {
        _validator = validator;
        _api = api;
        _download = download;
        _console = console;
        _progressService = progressService;
        _pack = pack;
        _series = series;
    }

    public async Task RunAsync(object options)
    {
        var opts = (RecommendOptions)options;
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
            _series.AddChapter(response, opts.Output, 1);

            var innerProgress = _progressService.NestToMaster(response.TotalPages, $"downloading id: {response.Id}");
            _download.HandleOnDownloadComplete((_, e) =>
            {
                innerProgress.Tick($"{e.Message} id: {response.Id}");
            });

            await _download.Start(_series.GetSeries().Chapters.FirstOrDefault());
            await _series.Close();
            
            if (opts.Pack)
            {
                await _pack.RunAsync(_series.GetSeries().Output, opts.Output, innerProgress);
            }
            
            _series.Reset();
            progress.Tick();
        }
    }
}
