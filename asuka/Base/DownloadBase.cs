using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using RestSharp;
using ShellProgressBar;
using asuka.Model;
using asuka.Internal.Cache;
using asuka.Utils;
using System.Diagnostics.CodeAnalysis;

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

    public static void Download(Response data, string outputPath, ProgressBar parentBar = null)
    {
      // Convert images to a URL string.
      List<ImageTaskItem> imageURLs = data.Images.Pages.Select((value, index) =>
      {
        string urlBase = "/galleries/" + data.MediaId + "/" + (index + 1);
        string fileName = (index + 1).ToString("D" + data.TotalPages.ToString().Length);

        switch (value.Type)
        {
          case "j":
            urlBase += ".jpg";
            fileName += ".jpg";
            break;
          case "p":
            urlBase += ".png";
            fileName += ".jpg";
            break;
          case "g":
            urlBase += ".gif";
            fileName += ".jpg";
            break;
          default:
            throw new NotImplementedException("New format is not yet implemented");
        }

        return new ImageTaskItem(urlBase, fileName);
      }).ToList();

      // Build destination path of images to store.
      string illegalRegex = new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars());
      Regex regexp = new Regex(string.Format("[{0}]", Regex.Escape(illegalRegex)));

      string folderName = data.Id.ToString() + " - " + regexp.Replace(data.Title.English, "");

      if (string.IsNullOrEmpty(outputPath))
      {
        outputPath = Environment.CurrentDirectory;
      }

      string destinationPath = Path.Join(outputPath, folderName);

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
      
      if (parentBar == null)
      {
        using var bar = new ProgressBar(data.TotalPages, "Downloading: " + data.Title.English, GlobalOptions.ParentBar);
        DownloadImage(imageURLs, data, destinationPath, bar);
      } else
      {
        using var bar = parentBar.Spawn(data.TotalPages, "Task: " + data.Title.English, GlobalOptions.ChildBar);
        DownloadImage(imageURLs, data, destinationPath, bar);
      }
    }

    private static void DownloadImage(List<ImageTaskItem> imageURLs, Response data, string outputPath, dynamic bar)
    {
      IntegrityManager Integrity = new IntegrityManager(data.Id);
      RestClient Client = new RestClient("https://i.nhentai.net/");

      Parallel.ForEach(imageURLs, new ParallelOptions { MaxDegreeOfParallelism = 2 }, task =>
      {
        string imagePath = Path.Join(outputPath, task.FileName);

        // Checks the hash of the image. If passes, it will skip the file.
        // Once checksum fails, delete the file.
        if (File.Exists(imagePath))
        {
          if (Integrity.CheckIntegrity(imagePath))
          {
            bar.Tick();
            return;
          }
        }

        RestRequest request = new RestRequest(task.ImageURL);
        IRestResponse response = Client.Execute(request);

        if (response.IsSuccessful)
        {
          File.WriteAllBytes(imagePath, response.RawBytes);
          Integrity.WriteIntegrity(imagePath);
          bar.Tick();
        }
        else
        {
          bar.Tick("Errored: " + data.Title.English);
        }
      });

      Integrity.SaveIntegrity();
    }
  }
}
