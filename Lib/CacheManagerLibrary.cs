using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using asukav2.Models;

namespace asukav2.Lib
{
  public class CacheManagerLibrary : IDisposable
  {
    private readonly CacheModelContext _context;
    private bool _disposed;
    private readonly List<ImageHash> _imageHashesBag;

    /// <summary>
    /// Initialises Cache Manager Context
    /// </summary>
    public CacheManagerLibrary()
    {
      _context = new CacheModelContext();

      // Initialise the bags.
      _imageHashesBag = new List<ImageHash>();
    }

    /// <summary>
    /// Adds image hash cache to the database. This is created so that the DownloadManager
    /// can run in multiple threads and will not slow the download. This add the image hash
    /// to the list and then when SaveChangesAsync(), it will add the changes to the SQLite
    /// database one by one.
    /// </summary>
    /// <param name="imageHashItem"></param>
    public void AddImageToBag(ImageHash imageHashItem) => _imageHashesBag.Add(imageHashItem);

    /// <summary>
    /// Adds image hash cache to the database.
    /// This ensures that the whatever is written to disk matches and can be skipped.
    /// </summary>
    /// <param name="imageHashItem">Image Hash item object</param>
    /// <returns></returns>
    private async Task AddImageToHashTableAsync(ImageHash imageHashItem)
    {
      // Find if there's hash already added to the database.
      var filter = await _context.ImageHashes.FirstOrDefaultAsync(
        c => c.DoujinCode == imageHashItem.DoujinCode && c.Image == imageHashItem.Image && c.Hash == imageHashItem.Hash);

      if (filter != null) return;
      
      await _context.ImageHashes.AddAsync(imageHashItem);
    }

    /// <summary>
    /// Gets the hash table of specific doujin
    /// </summary>
    /// <param name="code">Doujinshi Id.</param>
    /// <returns></returns>
    public async Task<List<ImageHash>> GetHashAsync(string code)
    {
      var filter = await _context.ImageHashes.Where(c => c.DoujinCode == code).ToListAsync();
      return filter;
    }

    /// <summary>
    /// Adds cache data to database
    /// </summary>
    /// <param name="code">nhentai 1-6 digit code</param>
    /// <param name="jsonResponse">nhentai's response but serialised as string.</param>
    /// <param name="token">Cancellation TOken</param>
    /// <returns></returns>
    public async Task AddDataToCacheAsync(string code, ResponseModel jsonResponse, CancellationToken token)
    {
      // Serialise the object.
      var serialized = JsonConvert.SerializeObject(jsonResponse);

      await _context.CacheData.AddAsync(new CacheModel
      {
        DoujinCode = code, JsonData = serialized
      }, token);
    }

    /// <summary>
    /// Writes all result to the cache.
    /// </summary>
    /// <param name="responses"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task WriteAllResultToCacheAsync(IEnumerable<ResponseModel> responses, CancellationToken token)
    {
      foreach (var response in responses)
      {
        await AddDataToCacheAsync(response.Id.ToString(), response, token);
      }
    }

    /// <summary>
    /// Retrieve doujin information stored in cache.
    /// </summary>
    /// <param name="code">nhentai 1-6 digit code</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public async Task<ResponseModel> GetDoujinInformationAsync(string code, CancellationToken token)
    {
      if (token.IsCancellationRequested)
      {
        token.ThrowIfCancellationRequested();
      }

      var filter = _context.CacheData.Where(x => x.DoujinCode == code);
      var data = await filter.FirstOrDefaultAsync(token);

      return data != null ? JsonConvert.DeserializeObject<ResponseModel>(data.JsonData) : null;
    }

    /// <summary>
    /// Add the total count to cache
    /// </summary>
    /// <param name="date">Time and Date of storing</param>
    /// <param name="count">Total doujin count</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    public async Task AddDoujinCountCacheAsync(DateTime date, int count, CancellationToken token)
    {
      if (token.IsCancellationRequested)
      {
        token.ThrowIfCancellationRequested();
      }

      var dateString = date.ToString("dd-MM-yyyy");
      await _context.DoujinTotalCounter.AddAsync(new DoujinCounter {Count = count, Date = dateString}, token);
    }

    public async Task<DoujinCounter> GetDoujinCountCacheAsync(DateTime date, CancellationToken token)
    {
      if (token.IsCancellationRequested)
      {
        token.ThrowIfCancellationRequested();
      }
      
      var data =  await _context.DoujinTotalCounter.FirstOrDefaultAsync(
        d => d.Date == date.ToString("dd-MM-yyyy"), token);

      return data;
    }

    /// <summary>
    /// Save Changes Asynchronously.
    /// </summary>
    /// <returns></returns>
    public async Task SaveChangesAsync()
    {
      // Commit the bags.
      foreach (var imageItem in _imageHashesBag)
      {
        await AddImageToHashTableAsync(imageItem);
      }

      await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (_disposed) return;

      // Dispose the DbContext
      if (disposing)
      {
        _context.Dispose();
        _imageHashesBag?.Clear();
      }
      _disposed = true;
    }

    /// <summary>
    /// Database Migration.
    /// </summary>
    /// <returns></returns>
    public static async Task AutoMigrateDatabaseAsync()
    {
      await using var context = new CacheModelContext();
      await context.Database.MigrateAsync();
    }

    public static bool IsDatabaseExists()
    {
      var databasePath = $"{Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory)!, "cache")}.db";
      var isDatabasePresent = File.Exists(databasePath);

      return isDatabasePresent;
    }
  }
}