using System.Threading.Tasks;
using asuka.Core.Models;

namespace asuka.Core.Chaptering;

public interface ISeriesFactory
{
    void AddChapter(GalleryResult result, string outputPath);
    void AddChapter(GalleryResult result, string outputPath, int chapterId);
    void Reset();
    Series GetSeries();
    Task Close();
}
