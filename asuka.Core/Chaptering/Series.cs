using asuka.Core.Models;

namespace asuka.Core.Chaptering;

public class Series
{
    private readonly string _output;
    private readonly IList<Chapter> _chapters;
    private readonly bool _isTachiyomiLayout;

    public Series(string outputPath, bool isTachiyomiLayout)
    {
        _chapters = new List<Chapter>();
        _output = outputPath;
        _isTachiyomiLayout = isTachiyomiLayout;
    }

    public void AddChapter(GalleryResult result, string source, int chapterId = 1)
    {
        var output = _isTachiyomiLayout
            ? Path.Combine(_output, $"ch{chapterId}")
            : _output;

        if (!Directory.Exists(output))
        {
            Directory.CreateDirectory(output);
        }

        _chapters.Add(new Chapter(result, output, chapterId, source));
    }

    public string GetOutput()
    {
        return _output;
    }

    public IList<Chapter> GetChapters()
    {
        return _chapters;
    }
}
