using asuka.Core.Compression;
using asuka.Core.Configuration;
using asuka.Core.Extensions;
using asuka.Core.Models;
using asuka.Core.Output.Progress;
using asuka.Core.Utilities;

namespace asuka.Core.Chaptering;

public class SeriesFactory : ISeriesFactory
{
    private Series _series;
    private readonly IConfigurationManager _configurationManager;
    private readonly IPackArchiveToCbz _pack;
    private readonly IProgressService _progress;

    public SeriesFactory(IConfigurationManager configurationManager, IPackArchiveToCbz pack, IProgressService progress)
    {
        _configurationManager = configurationManager;
        _pack = pack;
        _progress = progress;
    }

    public void AddChapter(GalleryResult result, string source, string outputPath)
    {
        AddChapter(result, outputPath, source, 1);
    }

    public void AddChapter(GalleryResult result, string source, string outputPath, int chapterId)
    {
        if (_series == null)
        {
            var destination = PathUtils.UsePathOrDefault(outputPath);
            var output = PathUtils.NormalizeJoin(destination, result.Title.GetTitle());
            _series = new Series(output, _configurationManager.GetValue("layout.tachiyomi") == "yes");
        }
        
        _series.AddChapter(result, source, chapterId);
    }

    public Series GetSeries() => _series;

    private async Task WriteMetadata()
    {
        var initialChapter = _series.Chapters.FirstOrDefault();
        if (initialChapter == null)
        {
            return;
        }

        if (_configurationManager.GetValue("layout.tachiyomi") == "yes")
        {
            await initialChapter.Data.WriteJsonMetadata(_series.Output);
            return;
        }

        await initialChapter.Data.WriteTextMetadata(_series.Output);
    }

    public async Task Close(IProgressProvider provider)
    {
        await WriteMetadata();

        if (provider != null)
        {
            var outputRoot = Path.Combine(_series.Output, "../");
            var files = Directory.GetFiles(_series.Output, "*.*", SearchOption.AllDirectories)
                .Select(x => (x, Path.GetRelativePath(outputRoot, x)))
                .ToArray();

            var progress = _progress.HookToInstance(provider, files.Length, "compressing...");

            _pack.HandleProgress((_, _) =>
            {
                progress.Tick();
            });
            await _pack.RunAsync(files, _series.Output);
        }
        
        // Finally close.
        _series = null;
    }
}
