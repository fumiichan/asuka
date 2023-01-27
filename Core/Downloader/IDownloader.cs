using System;
using System.Threading.Tasks;
using asuka.Core.Models;

namespace asuka.Core.Downloader;

public interface IDownloader
{
    public Action OnImageDownload { set; }
    public string DownloadRoot { get; }
    Task Start();
    Task Initialize(GalleryResult result, string outputPath, int chapter = -1);
}
