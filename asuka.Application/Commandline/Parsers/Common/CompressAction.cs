using System.IO;
using System.Linq;
using System.Threading.Tasks;
using asuka.Application.Output.Progress;
using asuka.Core.Chaptering.Models;
using asuka.Core.Compression;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Commandline.Parsers.Common;

public static class CompressAction
{
    public static async Task Compress(Series series, string output, IProgressProvider progress, ILogger logger)
    {
        var files = Directory.GetFiles(series.Output, "*.*", SearchOption.AllDirectories)
            .Select(x => new CompressionItem
            {
                FullPath = x,
                RelativePath = Path.GetRelativePath(output, x)
            })
            .ToArray();
        logger.LogInformation("Total of {Total} to be compressed", files.Length);
        
        var childProgress = progress.Spawn(1, "compressing...");
        await new CompressorBuilder()
            .AttachLogger(logger)
            .SetSeries(series)
            .SetEachCompleteListener(e =>
            {
                childProgress.Tick($"{e.Message}");
            })
            .Run(files);

        childProgress.Close();
    }
}
