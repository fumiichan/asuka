namespace asuka.Core.Downloader;

internal record PrepareResult
{
    public string FolderName { get; init; }
    public int Id { get; init; }
    public string DestinationPath { get; init; }
}
