using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Configuration;
using asuka.Core.Chaptering;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Requests;
using asuka.Output.ProgressService;
using asuka.Output.Writer;
using FluentValidation;

namespace asuka.Commandline.Parsers;

public class SeriesCreatorCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IConsoleWriter _console;
    private readonly IDownloader _downloader;
    private readonly IProgressService _progress;
    private readonly IPackArchiveToCbz _pack;
    private readonly IConfigurationManager _config;
    private readonly IValidator<SeriesCreatorCommandOptions> _validator;
    private readonly ISeriesFactory _series;

    public SeriesCreatorCommandService(
        IGalleryRequestService api,
        IConsoleWriter console,
        IDownloader downloader,
        IProgressService progress,
        IPackArchiveToCbz pack,
        IConfigurationManager config,
        IValidator<SeriesCreatorCommandOptions> validator,
        ISeriesFactory series)
    {
        _api = api;
        _console = console;
        _downloader = downloader;
        _progress = progress;
        _pack = pack;
        _config = config;
        _validator = validator;
        _series = series;
    }

    private async Task HandleArrayTask(IList<string> codes, string output, bool pack)
    {
        // Queue list of chapters.
        for (var i = 0; i < codes.Count; i++)
        {
            try
            {
                var response = await _api.FetchSingleAsync(codes[i]);
                _series.AddChapter(response, output, i + 1);
            }
            catch
            {
                _console.WarningLine($"Skipping: {codes[i]} because of an error.");
            }
        }

        // If there's no chapters (due to likely most of them failed to fetch metadata)
        // Quit immediately.
        if (_series.GetSeries() == null)
        {
            _console.SuccessLine("Nothing to do. Quitting...");
            return;
        }

        // Download chapters.
        var chapters = _series.GetSeries().Chapters;
        _progress.CreateMasterProgress(chapters.Count, "downloading series");

        foreach (var chapter in chapters)
        {
            try
            {
                var innerProgress =
                    _progress.NestToMaster(chapter.Data.TotalPages, $"downloading chapter {chapter.ChapterId}");
                _downloader.HandleOnDownloadComplete((_, _) =>
                {
                    innerProgress.Tick();
                });

                await _downloader.Start(chapter);
            }
            finally
            {
                _progress.GetMasterProgress().Tick();
            }
        }
        
        await _series.Close();

        if (pack)
        {
            await _pack.RunAsync(_series.GetSeries().Output, output, _progress.GetMasterProgress());
        }
    }

    public async Task RunAsync(object options)
    {
        var opts = (SeriesCreatorCommandOptions)options;

        var validationResult = await _validator.ValidateAsync(opts);
        if (!validationResult.IsValid)
        {
            _console.ValidationErrors(validationResult.Errors);
            return;
        }

        // Temporarily enable tachiyomi folder layout
        _config.SetValue("layout.tachiyomi", "yes");

        var list = opts.FromList.ToList();
        await HandleArrayTask(list, opts.Output, opts.Pack);
    }
}
