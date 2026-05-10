namespace asuka.Provider.Sdk;

public sealed class SearchInfo
{
    /// <summary>
    /// List of results.
    /// </summary>
    public required List<SearchResultObject> Result { get; init; }
    
    /// <summary>
    /// Total number of pages to paginate.
    /// </summary>
    public required int NumberOfPages { get; init; }
    
    /// <summary>
    /// Total number of results.
    /// </summary>
    public required int TotalPages { get; init; }
}

public sealed class SearchResultObject
{
    /// <summary>
    /// ID of the result.
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// Name of the gallery.
    /// </summary>
    public string Title { get; init; } = string.Empty;
}
