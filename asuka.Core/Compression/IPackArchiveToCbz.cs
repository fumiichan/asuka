using asuka.Core.Events;

namespace asuka.Core.Compression;

public interface IPackArchiveToCbz
{
    void HandleProgress(Action<object, ProgressEvent> e);
    Task RunAsync(IEnumerable<(string, string)> files, string targetFolder);
}
