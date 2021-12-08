using System.Threading.Tasks;
using asuka.Models;
using ShellProgressBar;

namespace asuka.Downloader;

public interface IDownloadService
{
  Task DownloadAsync(
    GalleryResult result,
    string outputPath,
    bool pack,
    bool useTachiyomiFolderLayout,
    IProgressBar progress);
}
