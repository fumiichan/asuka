using asuka.Core.Models;
using asuka.Core.Requests;

namespace asuka.Core.Chaptering.Models;

public record Chapter
{
    public GalleryResult Data { get; init; }
    public int Id { get; init; }
    public IGalleryImageRequestService Requestor { get; init; }
}
