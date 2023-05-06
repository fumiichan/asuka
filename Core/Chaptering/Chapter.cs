using asuka.Core.Models;

namespace asuka.Core.Chaptering;

public class Chapter
{
    public readonly GalleryResult Data;
    public readonly string Output;
    public readonly int ChapterId;

    public Chapter(GalleryResult result, string output, int chapterId)
    {
        Data = result;
        Output = output;
        ChapterId = chapterId;
    }
}
