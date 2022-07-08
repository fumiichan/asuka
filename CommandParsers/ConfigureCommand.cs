using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using asuka.Cloudflare;
using asuka.CommandOptions;

namespace asuka.CommandParsers;

public class ConfigureCommand : IConfigureCommand
{
    private static async Task SetDefaultCookies(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("File cannot be found on specified path.");
        }

        var appHomeDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".asuka");

        if (!Directory.Exists(appHomeDir))
        {
            Directory.CreateDirectory(appHomeDir);
        }

        await using var source = File.Open(path, FileMode.Open);
        await using var destination = File.Create(Path.Join(appHomeDir, "cookies.txt"));
        await source.CopyToAsync(destination);
    }

    public async Task RunAsync(ConfigureOptions opts)
    {
        if (!string.IsNullOrEmpty(opts.SetDefaultCookies))
        {
            await SetDefaultCookies(opts.SetDefaultCookies);
        }
    }
}
