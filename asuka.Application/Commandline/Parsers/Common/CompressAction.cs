using System.IO;
using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Output.Progress;
using asuka.Core.Chaptering.Models;
using asuka.Core.Compression;

namespace asuka.Application.Commandline.Parsers.Common;

public static class CompressAction
{
    public static async Task Compress(Series series, IProgressProvider progress, string output)
    {
        var files = Directory.GetFiles(series.Output, "*.*", SearchOption.AllDirectories)
            .Select(x => new CompressionItem
            {
                FullPath = x,
                RelativePath = Path.GetRelativePath(output, x)
            })
            .ToArray();
        
        var childProgress = progress.Spawn(1, "compressing...");
        await new CompressorBuilder()
            .SetSeries(series)
            .SetEachCompleteListener(e =>
            {
                childProgress.Tick($"{e.Message}");
            })
            .Run(files);

        childProgress.Close();
    }
}
