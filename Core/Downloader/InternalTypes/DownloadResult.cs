using ShellProgressBar;

namespace asuka.Core.Downloader.InternalTypes;

public record DownloadResult
{
    public string DestinationPath { get; init; }
    public IProgressBar ProgressBar { get; init; }
}
