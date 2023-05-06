using System;
using System.Threading.Tasks;
using asuka.Core.Chaptering;
using asuka.Core.Events;
using asuka.Core.Models;

namespace asuka.Core.Downloader;

public interface IDownloader
{
    void HandleOnProgress(Action<object, ProgressEvent> handler);
    Task Start(Chapter chapter);
}
