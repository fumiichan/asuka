namespace asuka.Core.Compression;

public record CompressionItem
{
    public string FullPath { get; init; }
    public string RelativePath { get; init; }
}
