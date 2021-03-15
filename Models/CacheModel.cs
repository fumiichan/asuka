using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace asukav2.Models
{
  public class CacheModelContext : DbContext
  {
    public DbSet<CacheModel> CacheData { get; set; }
    public DbSet<ImageHash> ImageHashes { get; set; }
    public DbSet<DoujinCounter> DoujinTotalCounter { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
     => options.UseSqlite(
        $"Data Source={Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "cache")}.db");
  }

  public class CacheModel
  {
    public int Id { get; set; }
    public string DoujinCode { get; set; }
    public string JsonData { get; set; }
  }

  public class DoujinCounter
  {
    public int Id { get; set; }
    public string Date { get; set; }
    public int Count { get; set; }
  }

  public class ImageHash
  {
    public int Id { get; set; }
    public string DoujinCode { get; set; }
    public string Image { get; set;  }
    public string Hash { get; set; }
  }
}
