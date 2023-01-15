using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using asuka.Output.ProgressService;
using ShellProgressBar;

namespace asuka.Core.Compression;

public class PackArchiveToCbz : IPackArchiveToCbz
{
    private readonly IProgressService _progressService;

    public PackArchiveToCbz(IProgressService progressService)
    {
        _progressService = progressService;
    }

    public async Task RunAsync(string targetFolder, string output, IProgressBar bar)
    {
        if (string.IsNullOrEmpty(output))
        {
            return;
        }

        var files = Directory.GetFiles(targetFolder, "*.*", SearchOption.AllDirectories)
            .Select(x => Path.GetRelativePath(output, x))
            .ToList();
        var childBar = _progressService.HookToInstance(bar, files.Count, $"compressing...");

        // Delete if file exists.
        if (File.Exists(output)) File.Delete(output);

        var destination = $"{targetFolder[..^1]}.cbz";
        await using var archiveToOpen = new FileStream(destination, FileMode.Create);
        using var archive = new ZipArchive(archiveToOpen, ZipArchiveMode.Create);
        
        // Recursively look for the files in the target directory.
        foreach (var file in files)
        {
            var entry = archive.CreateEntry(file);

            await using var writer = new BinaryWriter(entry.Open());
            var fileData = await File.ReadAllBytesAsync(Path.Combine(output, file));

            writer.Write(fileData);
            childBar.Tick($"compressing: {destination}");
        }
    }
}
