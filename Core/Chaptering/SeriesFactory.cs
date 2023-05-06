using System.Linq;
using System.Threading.Tasks;
using asuka.Configuration;
using asuka.Core.Extensions;
using asuka.Core.Models;
using asuka.Core.Utilities;

namespace asuka.Core.Chaptering;

public class SeriesFactory : ISeriesFactory
{
    private Series _series;
    private readonly IConfigurationManager _configurationManager;

    public SeriesFactory(IConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }

    public void AddChapter(GalleryResult result, string outputPath)
    {
        AddChapter(result, outputPath, 1);
    }

    public void AddChapter(GalleryResult result, string outputPath, int chapterId)
    {
        if (_series == null)
        {
            var output = PathUtils.NormalizeJoin(outputPath, result.Title.GetTitle());
            _series = new Series(output, _configurationManager.GetValue("layout.tachiyomi") == "yes");
        }
        
        _series.AddChapter(result, chapterId);
    }

    public Series GetSeries() => _series;

    public async Task Close()
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

    public void Reset()
    {
        _series = null;
    }
}
