namespace asuka.Provider.Sdk;

public sealed class SearchQuery
{
    public required List<string> SearchQueries { get; init; }
    public int PageNumber { get; init; }
    public string? Sort { get; init; }
}
