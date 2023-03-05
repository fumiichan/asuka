using System.Collections.Generic;
using System.Threading.Tasks;
using asuka.Output.ProgressService;
using asuka.Output.ProgressService.Providers;
using asuka.Output.ProgressService.Providers.Wrappers;
using ShellProgressBar;

namespace asuka.Core.Compression;

public interface IPackArchiveToCbz
{
    Task RunAsync(string targetFolder, string output, IProgressProvider bar);
}
