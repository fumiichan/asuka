using asuka.Sdk.Providers.Models;
using asuka.Sdk.Providers.Requests;

namespace asuka.Core.Chaptering.Models;

public record Chapter
{
    public GalleryResult Data { get; init; }
    public int Id { get; init; }
    public IGalleryImageRequestService Requestor { get; init; }
}
