using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using asuka.Api;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Extensions;
using asuka.Core.Models;
using asuka.Core.Output.Progress;
using asuka.Core.Utilities;

namespace asuka.Downloader;

public class SeriesDownloaderBuilder
{
    private readonly List<GalleryResult> _chapters;
    
    public required string Output { get; init; }
    public required IGalleryImage GalleryImage { get; init; }
    public required IProgressProvider Progress { get; init; }
    public int StartingChapter { get; init; } = 1;
    public bool Pack { get; init; }

    public SeriesDownloaderBuilder()
    {
        _chapters = new List<GalleryResult>();
    }

    public void AddChapter(GalleryResult chapter)
    {
        _chapters.Add(chapter);
    }

    public async Task Start()
    {
        if (_chapters.Count <= 0)
        {
            return;
        }

        var output = PathUtils.Join(Output, _chapters[0].Title.GetTitle());

        for (var i = 0; i < _chapters.Count; i++)
        {
            var childProgress = Progress
                .Spawn(_chapters[i].TotalPages, $"Downloading chapter {StartingChapter + i}")!;
            var downloader = new DownloadBuilder(_chapters[i], StartingChapter + i)
            {
                Output = output,
                Request = GalleryImage,
                OnEachComplete = _ =>
                {
                    childProgress.Tick();
                }
            };

            await downloader.Start();
            Progress.Tick();
        }
        
        // Write tachiyomi metadata
        await _chapters[0].WriteMetadata(Path.Combine(output, "details.json"));

        if (Pack)
        {
            await Compress.ToCbz(output, Progress);
        }
    }
}
