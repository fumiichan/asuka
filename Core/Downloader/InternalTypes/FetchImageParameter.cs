using asuka.Core.Models;

namespace asuka.Core.Downloader.InternalTypes;

internal record FetchImageParameter
{
    internal int MediaId { get; init; }
    internal GalleryImageResult Page { get; init; }
    internal int TaskId { get; init; }
    internal string DestinationPath { get; init; }
}
