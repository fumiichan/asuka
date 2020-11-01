using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using ShellProgressBar;
using asuka.Internal;
using asuka.Model;
using asuka.Utils;

namespace asuka.Base
{
  public class DownloadBase
  {
    private readonly Configuration Config;

    private struct DownloadItem
    {
      public string imageURL;
      public string filename;
      public string extension;
    }

    private readonly DownloadItem[] Images;
    private readonly Title DoujinshiTitles;
    private readonly int TotalPages;
    private readonly string DestinationPath;

    public DownloadBase(Response data, string outputPath)
    {
      // Use the current working directory if the path is empty.
      if (string.IsNullOrEmpty(outputPath))
      {
        outputPath = Environment.CurrentDirectory;
      }

      Config = new Configuration();
      string preferJapanese = Config.GetConfigurationValue("preferJapanese");
      bool useJapanese = bool.Parse(preferJapanese) && !string.IsNullOrEmpty(data.Title.Japanese);

      // Sanitize the path and the folder name.
      string illegalRegex = new string(Path.GetInvalidFileNameChars()) + ".";
      Regex regex = new Regex(string.Format("[{0}]", Regex.Escape(illegalRegex)));
      Regex multiSpacing = new Regex("[ ]{2,}");

      string folderName = regex.Replace($"{data.Id} - {(useJapanese ? data.Title.Japanese : data.Title.English)}", "");
      folderName = folderName.Trim();
      folderName = multiSpacing.Replace(folderName, " ");

      DestinationPath = Path.Join(outputPath, folderName);

      // Detect if the destination path exists.
      // If it exists, just use that directory instead.
      if (!Directory.Exists(DestinationPath))
      {
        Directory.CreateDirectory(DestinationPath);
      }

      // Write the metadata to the directory.
      DisplayDoujinMetadata.GenerateInfoFile(data, Path.Join(DestinationPath, "info.txt"));

      Images = data.Images.Pages.Select((value, index) =>
      {
        string urlPath = $"/galleries/{data.MediaId}/{(index + 1)}";
        string fileName = (index + 1).ToString($"D{data.TotalPages.ToString().Length}");
        string ext = value.Type switch
        {
          "j" => ".jpg",
          "p" => ".png",
          "g" => ".gif",
          _ => throw new NotImplementedException("New format is not supported.")
        };

        return new DownloadItem { imageURL = urlPath, extension = ext, filename = fileName };
      }).ToArray();

      DoujinshiTitles = data.Title;
      TotalPages = data.TotalPages;
    }

    public void Download(bool pack, IProgressBar progressBar = null)
    {
      using IProgressBar progress = progressBar == null
        ? new ProgressBar(TotalPages, $"Downloading: {DoujinshiTitles.English}", GlobalOptions.ParentBar)
        : (IProgressBar)progressBar.Spawn(TotalPages, $"Downloading: {DoujinshiTitles.English}", GlobalOptions.ParentBar);

      var client = new RestClient("https://i.nhentai.net/");

      int maxParallelTasks = int.Parse(Config.GetConfigurationValue("parallelImageDownload"));
      using SemaphoreSlim concurrency = new SemaphoreSlim(maxParallelTasks);

      List<Task> imageTasks = Images.Select(value =>
      {
        return Task.Factory.StartNew(() =>
        {
          concurrency.Wait();

          try
          {
            string imagePath = Path.Join(DestinationPath, $"{value.filename}{value.extension}");

            // Ensure the file is present on the disk and it is not empty.
            // Empty files could happen when there are power outage during writing or
            // maybe the disk got ejected or whatever the scenario.
            if (File.Exists(imagePath))
            {
              if (new FileInfo(imagePath).Length > 0)
              {
                progress.Tick();
                return;
              }
            }

            var request = new RestRequest($"{value.imageURL}{value.extension}");
            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
              File.WriteAllBytes(imagePath, response.RawBytes);
              progress.Tick();
            }
            else
            {
              progress.Tick($"Has Errors: {DoujinshiTitles.English}");
            }
          } 
          finally
          {
            concurrency.Release();
          }
        });
      }).ToList();

      Task.WaitAll(imageTasks.ToArray());

      // Compress the downloaded doujinshi just in case.
      if (pack)
      {
        Compress(progress);
      }
    }

    private void Compress(IProgressBar parent)
    {
      string zipArchivePath = DestinationPath + ".cbz";
      string directoryName = new DirectoryInfo(DestinationPath).Name;

      // Ensure the archive is valid when we gonna update
      if (File.Exists(zipArchivePath))
      {
        try
        {
          using var open = ZipFile.OpenRead(zipArchivePath);
        }
        catch (InvalidDataException)
        {
          File.Delete(zipArchivePath);
        }
      }

      // Determine which Archive mode we gonna use.
      ZipArchiveMode mode = File.Exists(zipArchivePath) ? ZipArchiveMode.Update : ZipArchiveMode.Create;

      using ZipArchive archive = ZipFile.Open(zipArchivePath, mode);

      // Create directory.
      archive.CreateEntry($"{directoryName}\\");

      // Initialise the Progress Bar.
      using var progress = parent.Spawn(TotalPages, "Compressing", GlobalOptions.ChildBar);

      // Compress files.
      string[] files = Directory.GetFiles(DestinationPath);
      foreach (var file in files)
      {
        archive.CreateEntryFromFile(file, $"{directoryName}\\{Path.GetFileName(file)}");
        progress.Tick();
      }
    }
  }
}
