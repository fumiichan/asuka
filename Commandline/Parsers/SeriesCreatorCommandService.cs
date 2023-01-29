using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Commandline.Options;
using asuka.Configuration;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Models;
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

    public SeriesCreatorCommandService(
        IGalleryRequestService api,
        IConsoleWriter console,
        IDownloader downloader,
        IProgressService progress,
        IPackArchiveToCbz pack,
        IConfigurationManager config,
        IValidator<SeriesCreatorCommandOptions> validator)
    {
        _api = api;
        _console = console;
        _downloader = downloader;
        _progress = progress;
        _pack = pack;
        _config = config;
        _validator = validator;
    }

    private async Task<string> DownloadTask(IList<GalleryResult> results)
    {
        _progress.CreateMasterProgress(results.Count, "downloading series");
        var masterProgress = _progress.GetMasterProgress();
        
        for (var i = 0; i < results.Count; i++)
        {
            var chapter = results[i];
            try
            {
                _downloader.CreateChapter(chapter, i + 1);
                
                var innerProgress = _progress.NestToMaster(chapter.TotalPages, $"downloading chapter {i + 1}");
                _downloader.SetOnImageDownload = () =>
                {
                    innerProgress.Tick();
                };

                await _downloader.Start();
            }
            finally
            {
                masterProgress.Tick();
            }
        }

        return _downloader.DownloadRoot;
    }

    private async Task<IList<GalleryResult>> GetChapterInformation(IEnumerable<string> ids)
    {
        // Queue list of chapters.
        var chapters = new List<GalleryResult>();
        foreach (var chapter in ids)
        {
            try
            {
                var galleryResponse = await _api.FetchSingleAsync(chapter);
                chapters.Add(galleryResponse);
            }
            catch
            {
                _console.WarningLine($"Skipping: {chapter} because of an error.");
            }
        }

        return chapters;
    }

    private async Task HandleArrayTask(IList<string> codes, string output, bool pack)
    {
        // Queue list of chapters.
        var chapters = await GetChapterInformation(codes);

        // If there's no chapters (due to likely most of them failed to fetch metadata)
        // Quit immediately.
        if (chapters.Count <= 0)
        {
            _console.SuccessLine("Nothing to do. Quitting...");
            return;
        }
        
        _downloader.CreateSeries(chapters[0].Title, output);
        var destinationPath = await DownloadTask(chapters);
        await _downloader.Final(chapters[0]);

        if (pack)
        {
            await _pack.RunAsync(destinationPath, output, _progress.GetMasterProgress());
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
