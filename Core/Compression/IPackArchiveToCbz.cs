using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using asuka.Core.Events;
using asuka.Output.ProgressService.Providers;

namespace asuka.Core.Compression;

public interface IPackArchiveToCbz
{
    void HandleProgress(Action<object, ProgressEvent> e);
    Task RunAsync(IEnumerable<(string, string)> files, string targetFolder);
}
