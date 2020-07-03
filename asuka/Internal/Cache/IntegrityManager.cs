using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace asuka.Internal.Cache
{
  #region Integrity Manager Model
  public struct IntegrityModel : IEquatable<IntegrityModel>
  {
    public string FileName;
    public string FileHash;

    public IntegrityModel(string fileName, string fileHash)
    {
      FileName = fileName;
      FileHash = fileHash;
    }

    public bool Equals(IntegrityModel other)
    {
      if (this.FileName == other.FileName && this.FileHash == other.FileHash)
        return true;

      return false;
    }
  }
  #endregion

  class IntegrityManager
  {
    private readonly string IntegrityCachePath;
    private readonly List<IntegrityModel> IntegrityData = new List<IntegrityModel>();
    private bool HasChanges;

    public IntegrityManager(int code)
    {
      string HomePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      string CacheDirPath = Path.Join(HomePath, "asuka");

      if (!Directory.Exists(CacheDirPath))
      {
        Directory.CreateDirectory(CacheDirPath);
      }

      IntegrityCachePath = Path.Join(CacheDirPath, code.ToString() + "-integrity.json");

      if (File.Exists(IntegrityCachePath))
      {
        try
        {
          using StreamReader read = new StreamReader(IntegrityCachePath);
          string json = read.ReadToEnd();

          IntegrityData = JsonConvert.DeserializeObject<List<IntegrityModel>>(json);
        } catch (Exception e)
        {
          Console.WriteLine("Failed to read integrity cache: {0}", e.Message);
        }
      }
    }

    public void WriteIntegrity(string filePath)
    {
      string FileName = Path.GetFileName(filePath);
      if (!IntegrityData.Any())
      {
        IntegrityModel file = IntegrityData.FirstOrDefault(v => v.FileName == FileName);
        if (string.IsNullOrEmpty(file.FileName))
        {
          IntegrityData.Add(new IntegrityModel(FileName, GetSHA256Hash(filePath)));
          HasChanges = true;
        }
      }
    }

    public bool CheckIntegrity(string filePath)
    {
      if (!IntegrityData.Any())
      {
        return false;
      }

      IntegrityModel file = IntegrityData.FirstOrDefault(v => v.FileName == Path.GetFileName(filePath));
      if (!string.IsNullOrEmpty(file.FileName))
      {
        return GetSHA256Hash(filePath) == file.FileHash;
      } else
      {
        return false;
      }
    }

    public void SaveIntegrity()
    {
      // Do not store again when there's no changes.
      if (!HasChanges)
      {
        return;
      }

      string json = JsonConvert.SerializeObject(IntegrityData);

      try
      {
        File.WriteAllText(IntegrityCachePath, json);
      } catch (Exception e)
      {
        Console.WriteLine("An error occured while saving integrity cache: {0}", e.Message);
      }
    }

    private static string GetSHA256Hash(string filePath)
    {
      using SHA256 hash = SHA256.Create();
      FileStream stream = File.OpenRead(filePath);
      stream.Position = 0;

      byte[] hashValue = hash.ComputeHash(stream);
      string outputHash = BitConverter.ToString(hashValue).Replace("-", string.Empty);

      return outputHash;
    }
  }
}
