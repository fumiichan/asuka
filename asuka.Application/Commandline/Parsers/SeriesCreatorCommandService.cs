using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Commandline.Options;
using asuka.Application.Configuration;
using asuka.Application.Output.Writer;
using asuka.Core.Chaptering;
using asuka.Core.Configuration;
using asuka.Core.Downloader;
using asuka.Core.Output.Progress;
using asuka.Core.Requests;
using FluentValidation;

namespace asuka.Application.Commandline.Parsers;

public class SeriesCreatorCommandService : ICommandLineParser
{
    private readonly IGalleryRequestService _api;
    private readonly IConsoleWriter _console;
    private readonly IDownloader _downloader;
    private readonly IProgressService _progress;
    private readonly IConfigurationManager _config;
    private readonly IValidator<SeriesCreatorCommandOptions> _validator;
    private readonly ISeriesFactory _series;

    public SeriesCreatorCommandService(
        IGalleryRequestService api,
        IConsoleWriter console,
        IDownloader downloader,
        IProgressService progress,
        IConfigurationManager config,
        IValidator<SeriesCreatorCommandOptions> validator,
        ISeriesFactory series)
    {
        _api = api;
        _console = console;
        _downloader = downloader;
        _progress = progress;
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
                var response = await _api.FetchSingle(codes[i]);
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
                _downloader.HandleOnProgress((_, _) =>
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
        
        await _series.Close(pack ? _progress.GetMasterProgress() : null);
    }

    public async Task Run(object options)
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
