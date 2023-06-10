using Refit;

namespace asuka.Providers.Nhentai.Api.Queries;

public record SearchQuery
{
    [AliasAs("query")]
    public string Queries { get; init; }

    [AliasAs("page")]
    public int PageNumber { get; init; }

    [AliasAs("sort")]
    public string Sort { get; init; }
}
