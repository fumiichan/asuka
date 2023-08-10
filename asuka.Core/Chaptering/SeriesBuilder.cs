using System.Collections.Immutable;
using asuka.Core.Chaptering.Models;
using asuka.Core.Utilities;
using asuka.Sdk.Providers.Extensions;
using asuka.Sdk.Providers.Models;
using asuka.Sdk.Providers.Requests;

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

    public Series Build(string language)
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
        
        // Write metadata on complete
        if (series.Chapters.Count > 0)
        {
            series.Chapters[0].Data.WriteJsonMetadata(series.Output, language);
        }

        return series;
    }
}
