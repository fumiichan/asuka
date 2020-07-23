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
using asuka.Internal.Cache;
using asuka.Model;
using asuka.Utils;

namespace asuka.Base
{
  public static class DownloadBase
  {
    public static void Download(Response data, bool pack, string outputPath, IProgressBar parentBar = null)
    {
      // Build destination path of images to store.
      string illegalRegex = new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars());
      Regex regex = new Regex(string.Format("[{0}]", Regex.Escape(illegalRegex)));

      if (string.IsNullOrEmpty(outputPath))
      {
        outputPath = Environment.CurrentDirectory;
      }

      Configuration config = new Configuration();
      string preferJapanese = config.GetConfigurationValue("preferJapanese");
      bool useJapanese = bool.Parse(preferJapanese) && !string.IsNullOrEmpty(data.Title.Japanese);

      string folderName = regex.Replace($"{data.Id} - {(useJapanese ? data.Title.Japanese : data.Title.English)}", "");
      string trimmedFolderName = folderName.Trim();
      string destinationPath = Path.Join(outputPath, trimmedFolderName);

      // Detect if the destination path exists.
      // If it exists, just use that directory instead.
      if (Directory.Exists(outputPath))
      {
        if (!Directory.Exists(destinationPath))
        {
          Directory.CreateDirectory(destinationPath);
        }
      }
      else
      {
        throw new DirectoryNotFoundException("The directory cannot be found.");
      }

      // Write the metadata to the directory.
      DisplayDoujinMetadata.GenerateInfoFile(data, Path.Join(destinationPath, "info.txt"));

      string zipArchivePath = Path.Join(outputPath, $"{trimmedFolderName}.cbz");
      ZipArchiveMode mode = File.Exists(zipArchivePath) ? ZipArchiveMode.Update : ZipArchiveMode.Create;

      using ZipArchive archive = pack ? ZipFile.Open(zipArchivePath, mode) : null;
      using IProgressBar progress = parentBar == null
        ? new ProgressBar(data.TotalPages, $"Downloading: {data.Title.English}", GlobalOptions.ParentBar)
        : (IProgressBar)parentBar.Spawn(data.TotalPages, $"Task: {data.Title.English}", GlobalOptions.ChildBar);

      int maxParallelTasks = int.Parse(config.GetConfigurationValue("parallelImageDownload"));
      using SemaphoreSlim concurrency = new SemaphoreSlim(maxParallelTasks);

      IntegrityManager integrity = new IntegrityManager(data.Id);
      RestClient Client = new RestClient("https://i.nhentai.net/");

      List<Task> imageTasks = data.Images.Pages.Select((value, index) =>
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

        return Task.Factory.StartNew(() =>
        {
          concurrency.Wait();

          try
          {
            string imagePath = Path.Join(destinationPath, $"{fileName}{ext}");

            // Checks the hash of the image. If passes, it will skip the file.
            // If checksum fails, re-download the image.
            if (File.Exists(imagePath) & integrity.CheckIntegrity(imagePath))
            {
              progress.Tick();
              archive?.CreateEntryFromFile(imagePath, $"{fileName}{ext}");
              return;
            }

            RestRequest request = new RestRequest($"{urlPath}{ext}");
            IRestResponse response = Client.Execute(request);

            if (response.IsSuccessful)
            {
              File.WriteAllBytes(imagePath, response.RawBytes);
              integrity.WriteIntegrity(imagePath);

              archive?.CreateEntryFromFile(imagePath, $"{fileName}{ext}");

              progress.Tick();
            }
            else
            {
              progress.Tick($"Errored: {data.Title.English}");
            }
          }
          finally
          {
            concurrency.Release();
          }
        });
      }).ToList();

      Task.WaitAll(imageTasks.ToArray());
      integrity.SaveIntegrity();
    }
  }
}
