using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace asuka.Compression
{
    public static class ZipExtension
    {
        public static async Task IsArchiveValid(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }
            
            await Task.Run(() =>
            {
                try
                {
                    using var zipFile = ZipFile.OpenRead(filePath);
                    _ = zipFile.Entries;
                }
                catch (Exception)
                {
                    File.Delete(filePath);
                }
            });
        }
    }
}