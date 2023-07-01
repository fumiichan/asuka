using System.Net;
using System.Reflection;
using System.Text.Json;

namespace asuka.Providers.Nhentai.Configuration;

public static class CookieConfiguration
{
    /// <summary>
    /// Loads cookies from file.
    /// </summary>
    public static IEnumerable<Cookie> LoadCookies()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        var configPath = Path.Combine(assemblyDir, "provider.nhentai-cookie.json");

        var cookieLists = new List<Cookie>();

        if (!File.Exists(configPath))
        {
            return cookieLists;
        }

        try
        {
            var file = File.ReadAllText(configPath);
            var cookieData = JsonSerializer.Deserialize<CookieDump[]>(file);

            if (cookieData is null)
            {
                return cookieLists;
            }

            cookieLists.AddRange(cookieData.Select(ApplyCookies));
        }
        catch
        {
            // ignored
        }

        return cookieLists;
    }

    private static Cookie ApplyCookies(CookieDump dump)
    {
        return new Cookie
        {
            Name = dump.Name,
            Domain = dump.Domain,
            HttpOnly = dump.HttpOnly,
            Secure = dump.Secure,
            Value = dump.Value
        };
    }
}
