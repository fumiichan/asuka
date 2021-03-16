using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using ShellProgressBar;
using RestSharp;
using asukav2.Config;
using asukav2.Models;

namespace asukav2.Lib
{
  public class DownloadManager
  {
    private readonly ResponseModel _data;
    private readonly string _taskName;
    private readonly string _destinationPath;
    private List<ImageHash> _integrityData;

    public DownloadManager(ResponseModel data, string outputPath)
    {
      // Use the current working directory if the path is empty.
      var destinationPath = Environment.CurrentDirectory;
      if (!string.IsNullOrEmpty(outputPath))
      {
        destinationPath = outputPath;
      }

      // Use other titles if they are present in other name properties.
      // Some doujinshis sometimes have missing names like in japanese or english equivalent.
      var doujinshiTitle = string.IsNullOrEmpty(data.Title.Japanese)
        ? (string.IsNullOrEmpty(data.Title.English) ? data.Title.Pretty : data.Title.English)
        : data.Title.Japanese;

      // Sanitise the path and the folder name.
      var illegalRegex = new string(Path.GetInvalidFileNameChars()) + ".";
      var regex = new Regex($"[{Regex.Escape(illegalRegex)}]");
      var multiSpacingRegex = new Regex("[ ]{2,}");

      var folderName = regex.Replace($"{data.Id} - {doujinshiTitle}", "");
      folderName = folderName.Trim();
      folderName = multiSpacingRegex.Replace(folderName, " ");

      _taskName = folderName;
      _destinationPath = Path.Join(destinationPath, folderName);
      _data = data;

      // Check if the output path exists.
      var exists = Directory.Exists(_destinationPath);
      if (!exists)
      {
        Directory.CreateDirectory(_destinationPath);
      }
    }

    /// <summary>
    /// Download the doujins.
    /// </summary>
    /// <param name="pack">Compress the download or not.</param>
    /// <param name="progress">Progress bar to display download and compressing progress</param>
    /// <param name="cache">Cache Manager Instance</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public async Task DownloadAsync(bool pack, IProgressBar progress, CacheManagerLibrary cache, CancellationToken token)
    {
      _integrityData = await cache.GetHashAsync(_data.Id.ToString());

      // Dump the text file.
      var fileContent = await DisplayOutputLibrary.StringifyResponseAsync(_data);
      await File.WriteAllTextAsync(Path.Combine(_destinationPath, "info.txt"), fileContent, token);

      // If the progress is null, we create a new one which is the parent bar.
      using var bar = progress == null
        ? new ProgressBar(_data.TotalPages, $"[queued] {_taskName}", GlobalProgressConfig.BarOptions)
        : (IProgressBar) progress.Spawn(_data.TotalPages, $"[queued] ${_taskName}",
          GlobalProgressConfig.BarOptions);

      var client = new RestClient("https://i.nhentai.net");

      var taskList = new List<Task>();
      using var throttler = new SemaphoreSlim(2);

      // DbContext is not thread safe. So we can't Task.WhenAll or Task.WaitAll.
      foreach (var (value, index) in _data.Images.Pages.Select((value, index) => (value, index)))
      {
        await throttler.WaitAsync(token);

        var referenceBar = bar;
        var referenceThrottler = throttler;
        taskList.Add(Task.Run(async () =>
        {
          await DownloadTaskAsync(value, index, referenceBar, client, cache, token);
          referenceThrottler.Release();
        }, token));
      }

      await Task.WhenAll(taskList);

      // Finally if pack is true, run the compression.
      if (pack)
      {
        await CompressTaskAsync(bar, token);
      }
    }

    /// <summary>
    /// Task that downloads the image.
    /// </summary>
    /// <param name="value">Page data</param>
    /// <param name="index">Index number in the array.</param>
    /// <param name="bar">IProgressBar instance</param>
    /// <param name="client">Rest Client</param>
    /// <param name="cache">Cache Manager Instance</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    private async Task DownloadTaskAsync(Pages value, int index, IProgressBar bar, IRestClient client,
      CacheManagerLibrary cache, CancellationToken token)
    {
      if (token.IsCancellationRequested)
      {
        token.ThrowIfCancellationRequested();
      }

      var ext = value.Type switch
      {
        "j" => ".jpg",
        "p" => ".png",
        "g" => ".gif",
        _ => throw new NotImplementedException("New format is not supported.")
      };

      var pageNumber = index + 1;

      var urlPath = $"/galleries/{_data.MediaId}/{pageNumber}{ext}";
      var filename = $"{pageNumber.ToString($"D{_data.TotalPages.ToString().Length}")}{ext}";
      var destinationPath = Path.Combine(_destinationPath, filename);

      // Check if the file exists and the hash matches on the database.
      var exists = await MatchIntegrityAsync(destinationPath, filename);
      if (exists)
      {
        bar.Tick($"[verifying] {_taskName}");
        return;
      }

      var request = new RestRequest(urlPath);
      var response = ExecuteRequest(client, request, bar);
      if (response.IsSuccessful)
      {
        await File.WriteAllBytesAsync(destinationPath, response.RawBytes, token);

        var hash = await ComputeSha256FromFileAsync(destinationPath);
        cache.AddImageToBag(
          new ImageHash {DoujinCode = _data.Id.ToString(), Hash = hash, Image = filename});

        bar.Tick($"[started] {_taskName}");
      }
    }

    /// <summary>
    /// Handles Creation and Updating of Zip Archives
    /// </summary>
    /// <param name="progress">IProgressBar instance</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    private async Task CompressTaskAsync(IProgressBar progress, CancellationToken token)
    {
      var fileList = Directory.GetFiles(_destinationPath);

      var archivePath = $"{_destinationPath}.cbz";
      if (File.Exists(archivePath))
      {
        try
        {
          using var archiveTest = ZipFile.OpenRead(archivePath);
        }
        catch
        {
          File.Delete(archivePath);
        }
      }

      var archiveMode = File.Exists(archivePath) ? ZipArchiveMode.Update : ZipArchiveMode.Create;

      await using var archiveStream = new FileStream(archivePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
      using var compress = new CompressManager(archiveStream, archiveMode, _taskName, _data.TotalPages, progress);

      foreach (var file in fileList)
      {
        if (token.IsCancellationRequested)
        {
          token.ThrowIfCancellationRequested();
        }
        await compress.InsertFromFileAsync(Path.Combine(_destinationPath, file));
      }
    }

    #region Utilities
    /// <summary>
    /// Computes SHA256 of a file.
    /// </summary>
    /// <param name="sourceFile">File to read and compute the hash</param>
    /// <returns></returns>
    private static async Task<string> ComputeSha256FromFileAsync(string sourceFile)
    {
      await using var stream = File.OpenRead(sourceFile);
      using var sha = new SHA256Managed();

      var checksum = await sha.ComputeHashAsync(stream);
      return BitConverter.ToString(checksum).Replace("-", string.Empty);
    }

    /// <summary>
    /// Matches the existing file's integrity
    /// </summary>
    /// <param name="targetFile">Target file to read and match against</param>
    /// <param name="filename">Used for looking up the file integrity on the database.</param>
    /// <returns></returns>
    private async Task<bool> MatchIntegrityAsync(string targetFile, string filename)
    {
      if (!File.Exists(targetFile))
      {
        return false;
      }

      // Fetch the hash of the file.
      var integrity = await ComputeSha256FromFileAsync(targetFile);

      // Find the integrity data.
      var fileIntegrity = _integrityData.FirstOrDefault((x) => x.Hash == integrity && x.Image == filename);
      return fileIntegrity != null;
    }

    /// <summary>
    /// Retry policy for RestClient configuration.
    /// </summary>
    /// <param name="progress">Just to display the request is being retried.</param>
    /// <returns></returns>
    private static RetryPolicy<IRestResponse> GetRetryPolicy(IProgressBar progress)
    {
      return Policy
        .HandleResult<IRestResponse>(r => r.IsSuccessful == false)
        .WaitAndRetryForever((retryAttempt) =>
        {
          var retryTime = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
          progress.WriteErrorLine($"Retrying request within {retryTime} seconds.");

          return retryTime;
        });
    }

    /// <summary>
    /// Wrapper to use Polly's Policy for Retries.
    /// </summary>
    /// <param name="client">RestSharp's client</param>
    /// <param name="request">RestSharp's request</param>
    /// <param name="progress">Just to display that request is being retried.</param>
    /// <returns></returns>
    private static IRestResponse ExecuteRequest(IRestClient client, IRestRequest request, IProgressBar progress)
    {
      return GetRetryPolicy(progress).Execute(() => client.Execute(request));
    }
    #endregion
  }
}
