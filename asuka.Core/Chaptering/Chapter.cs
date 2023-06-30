using asuka.Core.Models;

namespace asuka.Core.Chaptering;

public class Chapter
{
    public readonly GalleryResult Data;
    public readonly string Output;
    public readonly int ChapterId;
    public readonly string Source;

    public Chapter(GalleryResult result, string output, int chapterId, string source)
    {
        Data = result;
        Output = output;
        ChapterId = chapterId;
        Source = source;
    }
}
