using System.Collections.Generic;
using System.Threading.Tasks;
using ShellProgressBar;

namespace asuka.Core.Compression;

public interface IPackArchiveToCbz
{
    Task RunAsync(string folderName, IList<string> imageFiles, string output);
}
