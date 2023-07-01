using System.IO.Compression;
using asuka.Core.Events;

namespace asuka.Core.Compression;

public sealed class PackArchiveToCbz : IPackArchiveToCbz
{
    private EventHandler<ProgressEvent> _progressEvent;

    private void OnProgressEvent(ProgressEvent e)
    {
        _progressEvent?.Invoke(this, e);
    }

    public void HandleProgress(Action<object, ProgressEvent> e)
    {
        _progressEvent += (sender, @event) =>
        {
            e(sender, @event);
        };
    }

    public async Task Run(IEnumerable<CompressionItem> files, string targetFolder)
    {
        var destination = targetFolder.EndsWith("/") ? $"{targetFolder[..^1]}.cbz" : $"{targetFolder}.cbz";
        if (File.Exists(destination))
        {
            File.Delete(destination);
        }

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
