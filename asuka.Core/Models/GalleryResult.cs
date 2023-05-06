namespace asuka.Core.Models;

public record GalleryResult
{
    public int Id { get; init; }
    public int MediaId { get; init; }
    public GalleryTitleResult Title { get; init; }
    public IReadOnlyList<GalleryImageResult> Images { get; init; }
    public IReadOnlyList<string> Artists { get; init; }
    public IReadOnlyList<string> Parodies { get; init; }
    public IReadOnlyList<string> Characters { get; init; }
    public IReadOnlyList<string> Tags { get; init; }
    public IReadOnlyList<string> Categories { get; init; }
    public IReadOnlyList<string> Languages { get; init; }
    public IReadOnlyList<string> Groups { get; init; }
    public int TotalPages { get; init; }
}
