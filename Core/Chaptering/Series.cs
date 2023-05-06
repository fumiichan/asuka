using System.Collections.Generic;
using System.IO;
using asuka.Core.Models;

namespace asuka.Core.Chaptering;

public class Series
{
    public readonly string Output;
    public readonly IList<Chapter> Chapters;
    private readonly bool _isTachiyomiLayout;

    public Series(string outputPath, bool isTachiyomiLayout)
    {
        Chapters = new List<Chapter>();
        Output = outputPath;
        _isTachiyomiLayout = isTachiyomiLayout;
    }

    public void AddChapter(GalleryResult result, int chapterId = 1)
    {
        var output = _isTachiyomiLayout
            ? Path.Combine(Output, $"ch{chapterId}")
            : Output;

        if (!Directory.Exists(output))
        {
            Directory.CreateDirectory(output);
        }

        Chapters.Add(new Chapter(result, output, chapterId));
    }
}
