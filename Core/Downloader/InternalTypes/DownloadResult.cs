using System.Collections.Generic;

namespace asuka.Core.Downloader.InternalTypes;

public record DownloadResult
{
    public string FolderName { get; init; }
    public IList<string> ImageFiles { get; init; }
    public string DestinationPath { get; init; }
}
