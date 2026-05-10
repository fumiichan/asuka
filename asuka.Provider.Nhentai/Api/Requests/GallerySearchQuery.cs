using Refit;

namespace asuka.Provider.Nhentai.Api.Requests;

internal sealed class GallerySearchQuery
{
    [AliasAs("query")]
    public required string Queries { get; init; }

    [AliasAs("page")]
    public int PageNumber { get; init; } = 1;

    [AliasAs("sort")]
    public required string Sort { get; init; }
}
