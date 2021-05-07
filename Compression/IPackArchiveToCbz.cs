using System.Threading.Tasks;
using ShellProgressBar;

namespace asuka.Compression
{
    public interface IPackArchiveToCbz
    {
        Task RunAsync(string folderName, string[] imageFiles, string output, IProgressBar parentBar);
    }
}