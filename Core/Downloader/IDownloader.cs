using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using asuka.Core.Models;

namespace asuka.Core.Downloader;

public interface IDownloader
{
    public Action OnImageDownload { set; }
    public string DownloadRoot { get; }
    void CreateSeries(GalleryTitleResult title, string outputPath);
    void CreateChapter(GalleryResult result, int chapter = -1);
    Task Start();
    Task Finalize(GalleryResult result = null);
}
