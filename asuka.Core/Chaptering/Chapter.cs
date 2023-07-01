using asuka.Core.Models;

namespace asuka.Core.Chaptering;

public class Chapter
{
    private readonly GalleryResult _data;
    private readonly string _output;
    private readonly int _chapterId;
    private readonly string _source;

    public Chapter(GalleryResult result, string output, int chapterId, string source)
    {
        _data = result;
        _output = output;
        _chapterId = chapterId;
        _source = source;
    }

    public GalleryResult GetGalleryResult()
    {
        return _data;
    }

    public string GetOutput()
    {
        return _output;
    }

    public int GetChapterId()
    {
        return _chapterId;
    }

    public string GetSource()
    {
        return _source;
    }
}
