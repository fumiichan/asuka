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
  public static class DownloadBase
  {
    public static void Download(Response data, bool pack, string outputPath, IProgressBar parentBar = null)
    {
      // Use the current working directory if the path is empty.
      if (string.IsNullOrEmpty(outputPath))
      {
        outputPath = Environment.CurrentDirectory;
      }

      Configuration config = new Configuration();
      string preferJapanese = config.GetConfigurationValue("preferJapanese");
      bool useJapanese = bool.Parse(preferJapanese) && !string.IsNullOrEmpty(data.Title.Japanese);

      // Sanitize the path and the folder name.
      string illegalRegex = new string(Path.GetInvalidFileNameChars()) + ".";
      Regex regex = new Regex(string.Format("[{0}]", Regex.Escape(illegalRegex)));
      Regex multiSpacing = new Regex("[ ]{2,}");

      string folderName = regex.Replace($"{data.Id} - {(useJapanese ? data.Title.Japanese : data.Title.English)}", "");
      folderName = folderName.Trim();
      folderName = multiSpacing.Replace(folderName, " ");

      string destinationPath = Path.Join(outputPath, folderName);

      // Detect if the destination path exists.
      // If it exists, just use that directory instead.
      if (!Directory.Exists(destinationPath))
      {
        Directory.CreateDirectory(destinationPath);
      }

      // Write the metadata to the directory.
      DisplayDoujinMetadata.GenerateInfoFile(data, Path.Join(destinationPath, "info.txt"));

      string zipArchivePath = Path.Join(outputPath, $"{folderName}.cbz");
      ZipArchiveMode mode = File.Exists(zipArchivePath) ? ZipArchiveMode.Update : ZipArchiveMode.Create;

      using ZipArchive archive = pack ? ZipFile.Open(zipArchivePath, mode) : null;
      using IProgressBar progress = parentBar == null
        ? new ProgressBar(data.TotalPages, $"Downloading: {data.Title.English}", GlobalOptions.ParentBar)
        : (IProgressBar)parentBar.Spawn(data.TotalPages, $"Task: {data.Title.English}", GlobalOptions.ChildBar);

      int maxParallelTasks = int.Parse(config.GetConfigurationValue("parallelImageDownload"));
      using SemaphoreSlim concurrency = new SemaphoreSlim(maxParallelTasks);

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

            // Ensure the file is present on the disk and it is not empty.
            // Empty files could happen when there are power outage during writing or
            // maybe the disk got ejected or whatever the scenario.
            if (File.Exists(imagePath))
            {
              if (new FileInfo(imagePath).Length > 0)
              {
                progress.Tick();
                archive?.CreateEntryFromFile(imagePath, $"{fileName}{ext}");
                return;
              }
            }

            RestRequest request = new RestRequest($"{urlPath}{ext}");
            IRestResponse response = Client.Execute(request);

            if (response.IsSuccessful)
            {
              File.WriteAllBytes(imagePath, response.RawBytes);

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
    }
  }
}
