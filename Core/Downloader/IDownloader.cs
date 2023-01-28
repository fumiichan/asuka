using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using asuka.Core.Models;

namespace asuka.Core.Downloader;

public interface IDownloader
{
    public Action SetOnImageDownload { set; }
    public string DownloadRoot { get; }
    void CreateSeries(GalleryTitleResult title, string outputPath);
    void CreateChapter(GalleryResult result);
    void CreateChapter(GalleryResult result, int chapter);
    Task Start();
    Task Final();
    Task Final(GalleryResult result);
}
