using Refit;

namespace asuka.Provider.Koharu.Contracts.Queries;

internal sealed class ImageListQuery
{
    [AliasAs("v")]
    public long Version { get; set; }

    [AliasAs("w")]
    public int Width { get; set; } = 1280;
}
