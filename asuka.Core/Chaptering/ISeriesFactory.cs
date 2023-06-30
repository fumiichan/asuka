using asuka.Core.Models;
using asuka.Core.Output.Progress;

namespace asuka.Core.Chaptering;

public interface ISeriesFactory
{
    void AddChapter(GalleryResult result, string source, string outputPath);
    void AddChapter(GalleryResult result, string source, string outputPath, int chapterId);
    Series GetSeries();
    Task Close(IProgressProvider provider, bool disableMetaWriting);
}
