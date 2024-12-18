using Refit;

namespace asuka.Provider.Koharu.Contracts.Queries;

internal sealed class SearchParams
{
    [AliasAs("s")]
    public required string Query { get; init; }
}
