using Refit;

namespace asuka.Api.Queries;

public record SearchQuery
{
    [AliasAs("query")]
    public required string Queries { get; init; }

    [AliasAs("page")]
    public int PageNumber { get; init; }

    [AliasAs("sort")]
    public required string Sort { get; init; }
}
