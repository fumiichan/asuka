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
        AddChapter(result, source, outputPath, 1);
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
        var initialChapter = _series.GetChapters().FirstOrDefault();
        if (initialChapter == null)
        {
            return;
        }

        var galleryData = initialChapter.GetGalleryResult();
        if (_configurationManager.GetValue("layout.tachiyomi") == "yes")
        {
            await galleryData.WriteJsonMetadata(_series.GetOutput());
            return;
        }

        await galleryData.WriteTextMetadata(_series.GetOutput());
    }

    public async Task Close(IProgressProvider provider, bool disableMetaWriting)
    {
        if (!disableMetaWriting)
        {
            await WriteMetadata();
        }

        if (provider != null)
        {
            var outputRoot = Path.Combine(_series.GetOutput(), "../");
            var files = Directory.GetFiles(_series.GetOutput(), "*.*", SearchOption.AllDirectories)
                .Select(x => new CompressionItem
                {
                    FullPath = x,
                    RelativePath = Path.GetRelativePath(outputRoot, x)
                })
                .ToArray();

            var progress = _progress.HookToInstance(provider, files.Length, "compressing...");

            _pack.HandleProgress((_, _) =>
            {
                progress.Tick();
            });
            await _pack.Run(files, _series.GetOutput());
        }
        
        // Finally close.
        _series = null;
    }
}
