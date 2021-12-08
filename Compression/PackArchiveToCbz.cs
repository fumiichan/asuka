using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using asuka.Utils;
using ShellProgressBar;

namespace asuka.Compression;

public class PackArchiveToCbz : IPackArchiveToCbz
{
    public async Task RunAsync(string folderName, string[] imageFiles, string output, IProgressBar parentBar)
    {
        var progressTheme = ProgressBarConfiguration.CompressOption;
        var childBar = parentBar.Spawn(imageFiles.Length, "compressing...", progressTheme);

        await ValidateArchive(output, childBar).ConfigureAwait(false);

        var fileMode = File.Exists(output) ? FileMode.Open : FileMode.Create;
        var zipMode = File.Exists(output) ? ZipArchiveMode.Update : ZipArchiveMode.Create;

        await using var archiveToOpen = new FileStream(output, fileMode);
        using var archive = new ZipArchive(archiveToOpen, zipMode);

        archive.CreateEntry($"{folderName}\\");

        foreach (var image in imageFiles)
        {
            await AddOrUpdate(archive, folderName, image, zipMode)
                .ConfigureAwait(false);
            childBar.Tick();
        }
    }

    private static async Task AddOrUpdate(ZipArchive archive, string folderEntry, string imageFile, ZipArchiveMode mode)
    {
        var fileName = Path.GetFileName(imageFile);
        var entryName = $"{folderEntry}\\{fileName}";

        if (mode == ZipArchiveMode.Create)
        {
            await WriteEntry(archive, entryName, imageFile)
                .ConfigureAwait(false);
            return;
        }

        var archiveEntry = archive.GetEntry(entryName);
        archiveEntry?.Delete();

        await WriteEntry(archive, entryName, imageFile);
    }

    private static async Task WriteEntry(ZipArchive archive, string entryName, string inputFile)
    {
        var entry = archive.CreateEntry(entryName);
        await using var writer = new BinaryWriter(entry.Open());

        var fileContents = await File.ReadAllBytesAsync(inputFile)
            .ConfigureAwait(false);
        writer.Write(fileContents);
    }

    private static async Task ValidateArchive(string inputArchive, IProgressBar childBar)
    {
        if (!File.Exists(inputArchive))
        {
            return;
        }

        await Task.Run(() =>
        {
            if (IsArchiveValid(inputArchive))
            {
                return;
            }

            childBar.WriteErrorLine("Archive appears to be invalid. Deleting...");
            // Deletion might fail.
            File.Delete(inputArchive);
        }).ConfigureAwait(false);
    }

    private static bool IsArchiveValid(string inputArchive)
    {
        try
        {
            using var zipFile = ZipFile.OpenRead(inputArchive);
            _ = zipFile.Entries;

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
