using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using RestSharp;
using ShellProgressBar;
using asuka.Model;
using asuka.Internal.Cache;
using asuka.Utils;

namespace asuka.Base
{
  class DownloadBase
  {
    public struct ImageTaskItem : IEquatable<ImageTaskItem>
    {
      public string ImageURL;
      public string FileName;

      public ImageTaskItem(string imageURL, string fileName)
      {
        ImageURL = imageURL;
        FileName = fileName;
      }

      public bool Equals(ImageTaskItem other)
      {
        if (this.FileName == other.FileName && this.ImageURL == other.ImageURL)
        {
          return true;
        }

        return false;
      }
    }

    private readonly Response Data;
    private readonly string DestinationPath;
    private readonly string FolderName;

    /// <summary>
    /// Prepares the download by creating folders and writing metadata.
    /// </summary>
    /// <param name="data">nhentai response</param>
    /// <param name="outputPath">path where to save the download.</param>
    public DownloadBase(Response data, string outputPath)
    {
      Data = data;

      // Build destination path of images to store.
      string illegalRegex = new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars());
      Regex regexp = new Regex(string.Format("[{0}]", Regex.Escape(illegalRegex)));

      if (string.IsNullOrEmpty(outputPath))
      {
        outputPath = Environment.CurrentDirectory;
      }

      FolderName = regexp.Replace(data.Id.ToString() + " - " + data.Title.English, "");
      string destinationPath = Path.Join(outputPath, FolderName);

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

      DestinationPath = destinationPath;

      // Write the metadata to the directory.
      DisplayDoujinMetadata.GenerateInfoFile(data, Path.Join(destinationPath, "info.txt"));
    }

    /// <summary>
    /// Downloads the doujin.
    /// </summary>
    /// <param name="pack">Pack the downloaded archive as cbz</param>
    /// <param name="parentBar">Parent progress bar</param>
    public void Download(bool pack, ProgressBar parentBar = null)
    {
      // Convert images to a URL string.
      List<ImageTaskItem> imageURLs = Data.Images.Pages.Select((value, index) =>
      {
        string urlBase = "/galleries/" + Data.MediaId + "/" + (index + 1);
        string fileName = (index + 1).ToString("D" + Data.TotalPages.ToString().Length);
        string ext = value.Type switch
        {
          "j" => ".jpg",
          "p" => ".png",
          "g" => ".gif",
          _ => throw new NotImplementedException("New format is not yet implemented"),
        };

        return new ImageTaskItem(urlBase + ext, fileName + ext);
      }).ToList();

      string parentPath = Directory.GetParent(DestinationPath).FullName;
      string destinationPath = Path.Join(parentPath, FolderName + ".cbz");
      ZipArchiveMode mode = File.Exists(destinationPath) ? ZipArchiveMode.Update : ZipArchiveMode.Create;

      using ZipArchive archive = pack ? ZipFile.Open(destinationPath, mode) : null;
      using ChildProgressBar bar = parentBar?.Spawn(Data.TotalPages, "Task: " + Data.Title.English, GlobalOptions.ChildBar);

      DownloadImages(imageURLs, bar, archive);
    }

    /// <summary>
    /// Downloads the images.
    /// </summary>
    /// <param name="imageURLs">Image URLs to download</param>
    /// <param name="bar">Progress bar instance</param>
    private void DownloadImages(List<ImageTaskItem> imageURLs, IProgressBar bar = null, ZipArchive archive = null)
    {
      IntegrityManager Integrity = new IntegrityManager(Data.Id);
      RestClient Client = new RestClient("https://i.nhentai.net/");

      using IProgressBar progress = bar ?? new ProgressBar(Data.TotalPages, "Downloading: " + Data.Title.English, GlobalOptions.ParentBar);

      Parallel.ForEach(imageURLs, new ParallelOptions { MaxDegreeOfParallelism = 2 }, task =>
      {
        string imagePath = Path.Join(DestinationPath, task.FileName);

        // Checks the hash of the image. If passes, it will skip the file.
        // Once checksum fails, delete the file.
        if (File.Exists(imagePath))
        {
          if (Integrity.CheckIntegrity(imagePath))
          {
            progress.Tick("Passed: " + Data.Title.English);
            return;
          }
        }

        RestRequest request = new RestRequest(task.ImageURL);
        IRestResponse response = Client.Execute(request);

        if (response.IsSuccessful)
        {
          File.WriteAllBytes(imagePath, response.RawBytes);
          Integrity.WriteIntegrity(imagePath);

          archive?.CreateEntryFromFile(imagePath, Path.GetFileName(imagePath));

          progress.Tick("Downloading: " + Data.Title.English);
        }
        else
        {
          progress.Tick("Errored: " + Data.Title.English);
        }
      });

      Integrity.SaveIntegrity();

      if (archive != null)
      {
        Directory.Delete(DestinationPath);
      }
    }
  }
}
