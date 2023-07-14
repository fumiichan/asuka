using System.IO.Compression;
using asuka.Core.Events;
using Microsoft.Extensions.Logging;

namespace asuka.Core.Compression;

internal sealed class Compressor : ProgressEmittable
{
    private readonly ILogger _logger;

    public Compressor(ILogger logger)
    {
        _logger = logger;
    }

    public async Task Run(IEnumerable<CompressionItem> files, string destination)
    {
        try
        {
            _logger.LogInformation("Archive to compress: {Destination}", destination);
            await using var archiveToOpen = new FileStream(destination, FileMode.Create);

            using var archive = new ZipArchive(archiveToOpen, ZipArchiveMode.Create);

            // Recursively look for the files in the target directory.
            foreach (var file in files)
            {
                _logger.LogInformation("Entry created for compression: {RelativePath}", file.RelativePath);
                var entry = archive.CreateEntry(file.RelativePath);

                await using var writer = new BinaryWriter(entry.Open());
                var fileData = await File.ReadAllBytesAsync(file.FullPath);

                writer.Write(fileData);
                OnProgressEvent(new ProgressEventArgs("compressing"));
                _logger.LogInformation("Compression successful for {RelativePath}", file.RelativePath);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("An exception occured in writing ZIP archive: {@Excption}", e);
            OnProgressEvent(new ProgressEventArgs("failed to compress"));
        }
    }
}
