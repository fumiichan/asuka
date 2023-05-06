using asuka.Core.Chaptering;
using asuka.Core.Events;

namespace asuka.Core.Downloader;

public interface IDownloader
{
    void HandleOnProgress(Action<object, ProgressEvent> handler);
    Task Start(Chapter chapter);
}
