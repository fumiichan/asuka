using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace asuka.Provider.Sdk.Utilities;

internal static class CookieParsers
{
    /// <summary>
    /// Gets the cookie dump from file relative to the assembly path
    /// </summary>
    /// <param name="type">The assembly to be used as reference</param>
    /// <param name="fileName">Custom file name of the dump</param>
    /// <returns></returns>
    public static List<Cookie> GetFromFileRelativeToType(Type type, string fileName = "cookies.txt")
    {
        var assemblyRoot = Path.GetDirectoryName(Assembly.GetAssembly(type)?.Location);
        if (string.IsNullOrEmpty(assemblyRoot))
        {
            return [];
        }
        
        var path = Path.Combine(assemblyRoot, Path.GetFileName(fileName));
        if (!File.Exists(path))
        {
            return [];
        }

        if (TryParseJsonExport(path, out var jsonCookies))
        {
            return jsonCookies;
        }

        return TryParseNetscapeNavigatorExport(path, out var navigatorCookies)
            ? navigatorCookies
            : [];
    }
    
    private static bool TryParseJsonExport(string filePath, out List<Cookie> cookies)
    {
        if (!File.Exists(filePath))
        {
            cookies = [];
            return false;
        }
        
        try
        {
            var file = File.ReadAllText(filePath, Encoding.UTF8);
            var data = JsonSerializer.Deserialize<JsonCookie[]>(file);

            if (data == null)
            {
                cookies = [];
                return false;
            }

            var exportedCookies = new List<Cookie>();
            foreach (var cookie in data)
            {
                exportedCookies.Add(new Cookie
                {
                    Name = cookie.Name,
                    Value = cookie.Value,
                    Domain = cookie.Domain,
                    HttpOnly = cookie.HttpOnly,
                    Secure = cookie.Secure,
                    Path = cookie.Path
                });
            }

            cookies = exportedCookies;
            return true;
        }
        catch
        {
            cookies = [];
            return false;
        }
    }

    private static bool TryParseNetscapeNavigatorExport(string filePath, out List<Cookie> cookies)
    {
        if (!File.Exists(filePath))
        {
            cookies = [];
            return false;
        }

        var exportedCookies = new List<Cookie>();
        foreach (var line in File.ReadAllLines(filePath))
        {
            // Skip line starts with #. These are comments.
            if (line.StartsWith('#'))
            {
                continue;
            }

            var fields = line.Split('\t').ToList();
            
            // If the field count isn't 7, ignore the line. Just to be safe.
            if (fields.Count < 7)
            {
                continue;
            }

            var host = fields[0];
            var path = fields[2];
            var isSecure = bool.Parse(fields[3]);
            var name = fields[5];
            var value = fields[6];

            var cookie = new Cookie
            {
                Name = name,
                Value = value,
                Domain = host,
                Secure = isSecure,
                Path = path
            };
            
            exportedCookies.Add(cookie);
        }

        cookies = exportedCookies;
        return true;
    }
}
