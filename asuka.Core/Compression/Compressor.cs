using System.IO.Compression;
using asuka.Core.Events;

namespace asuka.Core.Compression;

internal sealed class Compressor : ProgressEmittable
{
    public async Task Run(IEnumerable<CompressionItem> files, string destination)
    {
        await using var archiveToOpen = new FileStream(destination, FileMode.Create);
        using var archive = new ZipArchive(archiveToOpen, ZipArchiveMode.Create);
        
        // Recursively look for the files in the target directory.
        foreach (var file in files)
        {
            var entry = archive.CreateEntry(file.RelativePath);

            await using var writer = new BinaryWriter(entry.Open());
            var fileData = await File.ReadAllBytesAsync(file.FullPath);

            writer.Write(fileData);
            OnProgressEvent(new ProgressEvent("compressing"));
        }
    }
}
