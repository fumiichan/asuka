using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;

namespace asuka.Application.Services.Downloader.Compression;

internal static class Compress
{
    private class FilePath
    {
        public required string Relative { get; init; }
        public required string Full { get; init; }
    }
    
    public static async Task ToCbz(string folder)
    {
        var directory = new DirectoryInfo(folder);
        var parent = Directory.GetParent(directory.FullName)
                     ?? new DirectoryInfo(Directory.GetDirectoryRoot(directory.FullName));

        var files = directory.EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Select(x => new FilePath
            {
                Relative = Path.GetRelativePath(parent.FullName, x.FullName),
                Full = x.FullName
            })
            .ToList();
        var outputFile = Path.Combine(parent.FullName, directory.Name + ".cbz");
        
        // Delete existing file
        if (File.Exists(outputFile))
        {
            File.Delete(outputFile);
        }
        
        await using var stream = new FileStream(outputFile, FileMode.Create);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

        foreach (var file in files)
        {
            var entry = archive.CreateEntry(file.Relative);

            await using var writer = new BinaryWriter(entry.Open());
            var data = await File.ReadAllBytesAsync(file.Full);

            writer.Write(data);
        }
        
        AnsiConsole.MarkupLine($"[chartreuse1]Compression done: {Path.GetDirectoryName(folder)}[/]");
    }
}
