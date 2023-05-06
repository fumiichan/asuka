using System;
using System.Threading.Tasks;
using asuka.Core.Chaptering;
using asuka.Core.Events;
using asuka.Core.Models;

namespace asuka.Core.Downloader;

public interface IDownloader
{
    void HandleOnDownloadComplete(Action<object, ProgressEvent> handler);
    Task Start(Chapter chapter);
}
