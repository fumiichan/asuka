namespace asuka.Provider.Sdk;

public sealed class Series
{
    /// <summary>
    /// Title of the Gallery
    /// </summary>
    public required string Title { get; init; }
    
    /// <summary>
    /// Authors of the gallery
    /// </summary>
    public required List<string> Authors { get; init; }
    
    /// <summary>
    /// Artists (Can be synonymous  to Authors)
    /// </summary>
    public required List<string> Artists { get; init; }
    
    /// <summary>
    /// Genres/Tags of the gallery
    /// </summary>
    public required List<string> Genres { get; init; }
    
    /// <summary>
    /// Chapters of the gallery
    /// </summary>
    public required List<Chapter> Chapters { get; init; }
    
    /// <summary>
    /// Status of the series
    /// </summary>
    public SeriesStatus Status { get; init; }
}