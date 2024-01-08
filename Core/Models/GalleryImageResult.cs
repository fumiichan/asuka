namespace asuka.Core.Models;

public record GalleryImageResult
{
    public required string ServerFilename { get; init; }
    public required string Filename { get; init; }
}
