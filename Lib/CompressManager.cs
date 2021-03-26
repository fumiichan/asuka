using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ShellProgressBar;
using asukav2.Config;

namespace asukav2.Lib
{
  public class CompressManager : ZipArchive
  {
    private readonly IProgressBar _progress;
    private bool _disposed;
    private readonly string _entryName;
    private readonly bool _createMode;

    /// <summary>
    /// Compress Doujinshi.
    /// </summary>
    /// <param name="archive">Archive stream to use.</param>
    /// <param name="mode">Archive mode.</param>
    /// <param name="entryName">Name of the current task</param>
    /// <param name="totalLength">Total amount of files to compress</param>
    /// <param name="progress">ShellProgressBar instance</param>
    public CompressManager(Stream archive, ZipArchiveMode mode, string entryName, int totalLength, 
      IProgressBar progress) : base(archive, mode)
    {
      _progress = progress.Spawn(totalLength, $"[zip] {entryName}", GlobalProgressConfig.CompressBarOption);
      _entryName = entryName;

      _createMode = mode == ZipArchiveMode.Create;

      // Create empty directory inside the archive.
      CreateEntry($"{entryName}/");
    }

    /// <summary>
    /// Insert a file to the archive
    /// </summary>
    /// <param name="filePath">File to insert to the archive</param>
    public async Task InsertFromFileAsync(string filePath)
    {
      var entry = await CreateOrOverwriteEntryAsync($"{_entryName}/{Path.GetFileName(filePath)}");
      var data = await File.ReadAllBytesAsync(filePath);

      await Task.Run(() =>
      {
        using var writer = new BinaryWriter(entry.Open());
        writer.Write(data);
      });
      
      _progress.Tick();
    }

    /// <summary>
    /// Creates or Overwrites
    /// </summary>
    /// <param name="entryName">Entry to overwrite.</param>
    private async Task<ZipArchiveEntry> CreateOrOverwriteEntryAsync(string entryName)
    {
      var task = await Task.Run(() =>
      {
        // Just create the entries if the mode is create.
        if (_createMode)
        {
          return CreateEntry(entryName);
        }

        var archiveEntry = GetEntry(entryName);
        if (archiveEntry == null)
        {
          return CreateEntry(entryName);
        }

        // Delete the entry to overwrite it.
        archiveEntry.Delete();
        return CreateEntry(entryName);
      });

      return task;
    }

    protected override void Dispose(bool disposing)
    {
      if (_disposed)
      {
        return;
      }

      _progress.Dispose();
      _disposed = true;

      // Dispose.
      Dispose();
    }
  }
}
