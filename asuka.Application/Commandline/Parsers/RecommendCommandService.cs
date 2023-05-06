using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Output.Writer;
using asuka.Application.Utilities;
using asuka.Core.Chaptering;
using asuka.Core.Downloader;
using asuka.Core.Mappings;
using asuka.Core.Output.Progress;
using asuka.Core.Requests;
using FluentValidation;
using Sharprompt;

namespace asuka.Application.Commandline.Parsers;

public class RecommendCommandService : ICommandLineParser
{
    private readonly IValidator<RecommendOptions> _validator;
    private readonly IGalleryRequestService _api;
    private readonly IDownloader _download;
    private readonly IConsoleWriter _console;
    private readonly IProgressService _progressService;
    private readonly ISeriesFactory _series;

    public RecommendCommandService(
        IValidator<RecommendOptions> validator,
        IGalleryRequestService api,
        IDownloader download,
        IConsoleWriter console,
        IProgressService progressService,
        ISeriesFactory series)
    {
        _validator = validator;
        _api = api;
        _download = download;
        _console = console;
        _progressService = progressService;
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
        var selection = await Selection.MultiSelect(responses);

        // Initialise the Progress bar.
        _progressService.CreateMasterProgress(selection.Count, $"[task] recommend from id: {opts.Input}");
        var progress = _progressService.GetMasterProgress();

        foreach (var response in selection)
        {
            _series.AddChapter(response, opts.Output, 1);

            var innerProgress = _progressService.NestToMaster(response.TotalPages, $"downloading id: {response.Id}");
            _download.HandleOnProgress((_, e) =>
            {
                innerProgress.Tick($"{e.Message} id: {response.Id}");
            });

            await _download.Start(_series.GetSeries().Chapters.FirstOrDefault());
            await _series.Close(opts.Pack ? innerProgress : null);
            
            progress.Tick();
        }
    }
}
