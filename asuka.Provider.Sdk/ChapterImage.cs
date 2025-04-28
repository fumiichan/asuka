namespace asuka.Provider.Sdk;

public sealed class ChapterImage
{
    /// <summary>
    /// URL of your image to download
    /// </summary>
    public required string RemotePath { get; init; }
        
    /// <summary>
    /// Name of the file to be saved into the disk
    /// </summary>
    public required string Filename { get; init; }
        
    /// <summary>
    /// Add an optional request headers needed for your request
    /// </summary>
    public Dictionary<string, string> RequestHeaders { get; set; } = new();
}
