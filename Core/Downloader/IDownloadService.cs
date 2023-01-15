using System.Threading.Tasks;
using asuka.Core.Downloader.InternalTypes;
using asuka.Core.Models;
using ShellProgressBar;

namespace asuka.Core.Downloader;

public interface IDownloadService
{
    Task<DownloadResult> DownloadAsync(GalleryResult result, string outputPath);
}
