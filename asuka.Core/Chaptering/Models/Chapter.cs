using asuka.Core.Models;

namespace asuka.Core.Chaptering.Models;

public record Chapter
{
    public GalleryResult Data { get; init; }
    public int Id { get; init; }
    public string Source { get; init; }
}
