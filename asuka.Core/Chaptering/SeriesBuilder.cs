using System.Collections.Immutable;
using asuka.Core.Chaptering.Models;
using asuka.Core.Extensions;
using asuka.Core.Models;
using asuka.Core.Requests;
using asuka.Core.Utilities;

namespace asuka.Core.Chaptering;

public class SeriesBuilder
{
    private readonly IList<Chapter> _chapters = new List<Chapter>();
    private string _output = string.Empty;

    public SeriesBuilder AddChapter(GalleryResult result, IGalleryImageRequestService requestor)
    {
        AddChapter(result, requestor, 1);
        return this;
    }

    public SeriesBuilder AddChapter(GalleryResult result, IGalleryImageRequestService requestor, int id)
    {
        _chapters.Add(new Chapter
        {
            Data = result,
            Id = id,
            Requestor = requestor
        });

        return this;
    }

    public SeriesBuilder SetOutput(string outputPath)
    {
        _output = PathUtils.UsePathOrDefault(outputPath);
        return this;
    }

    public Series Build()
    {
        var series = new Series
        {
            Chapters = _chapters.ToImmutableArray()
        };

        if (series.Chapters.Count == 0)
        {
            series.Output = _output;
        }

        series.Output = PathUtils
            .NormalizeJoin(_output, _chapters[0].Data.Title.GetTitle());

        // Create directory for downloading.
        if (!Directory.Exists(series.Output))
        {
            Directory.CreateDirectory(series.Output);
        }

        return series;
    }
}
