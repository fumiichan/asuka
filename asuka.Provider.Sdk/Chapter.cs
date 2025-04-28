namespace asuka.Provider.Sdk;

public sealed class Chapter
{
    public required int Id { get; init; }
    public List<ChapterImage> Pages { get; init; } = [];
}
