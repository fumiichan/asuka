using System.Collections.Generic;

namespace asuka.Core.Models;

public record GalleryResult
{
    public int Id { get; init; }
    public int MediaId { get; init; }
    public required GalleryTitleResult Title { get; init; }
    public required IReadOnlyList<GalleryImageResult> Images { get; init; }
    public required IReadOnlyList<string> Artists { get; init; }
    public required IReadOnlyList<string> Parodies { get; init; }
    public required IReadOnlyList<string> Characters { get; init; }
    public required IReadOnlyList<string> Tags { get; init; }
    public required IReadOnlyList<string> Categories { get; init; }
    public required IReadOnlyList<string> Languages { get; init; }
    public required IReadOnlyList<string> Groups { get; init; }
    public int TotalPages { get; init; }
}
