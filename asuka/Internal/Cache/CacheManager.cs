using System;
using System.IO;
using Newtonsoft.Json;
using asuka.Model;

namespace asuka.Internal.Cache
{
  class CacheManager
  {
    private readonly string CachePath;

    /// <summary>
    /// Initialize Cache Manager cache.
    /// Prepares the directory to store the nhentai responses.
    /// </summary>
    /// <param name="doujinCode">The 1-6 nhentai code.</param>
    public CacheManager(int doujinCode)
    {
      string HomePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      string CacheDirPath = Path.Join(HomePath, "asuka");

      if (!Directory.Exists(CacheDirPath))
      {
        Directory.CreateDirectory(CacheDirPath);
      }

      CachePath = Path.Join(CacheDirPath, $"{doujinCode}.json");
    }

    /// <summary>
    /// Writes the nhentai response to cache.
    /// </summary>
    /// <param name="data">nhentai response data</param>
    public void WriteCache (Response data)
    {
      string json = JsonConvert.SerializeObject(data);

      try
      {
        File.WriteAllText(CachePath, json);
      } catch (Exception e)
      {
        Console.WriteLine("Unable to write cache: {0}", e.Message);
      }
    }

    /// <summary>
    /// Reads the cache if present.
    /// </summary>
    /// <returns>nhentai response data or null</returns>
    public Response ReadCache ()
    {
      if (File.Exists(CachePath))
      {
        using StreamReader read = new StreamReader(CachePath);
        string json = read.ReadToEnd();

        try
        {
          return JsonConvert.DeserializeObject<Response>(json);
        } catch (Exception e)
        {
          Console.WriteLine("Unable to read cache: {0}", e.Message);
          return null;
        }
      } else
      {
        return null;
      }
    }
  }
}
