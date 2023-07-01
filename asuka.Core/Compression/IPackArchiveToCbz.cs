using asuka.Core.Events;

namespace asuka.Core.Compression;

public interface IPackArchiveToCbz
{
    void HandleProgress(Action<object, ProgressEvent> e);
    Task Run(IEnumerable<CompressionItem> files, string targetFolder);
}
