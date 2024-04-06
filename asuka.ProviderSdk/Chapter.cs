namespace asuka.ProviderSdk;

using System.Collections.Generic;

public sealed class Chapter
{
    public sealed class ChapterImages
    {
        public required string ImageRemotePath { get; init; }
        public required string Filename { get; init; }
    } 
    
    public required int Id { get; init; }
    public List<ChapterImages> Pages { get; init; }
}
