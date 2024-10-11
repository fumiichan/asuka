using System.Reflection;

namespace asuka.Provider.Common;

public static class HttpClientFactory
{
    /// <summary>
    /// Creates an HTTP Client
    /// </summary>
    /// <param name="hostname"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public static HttpClient CreateClientFromProvider<T>(
        string hostname,
        Dictionary<string, string>? customHeaders = null)
    {
        var userAgent = GetUserAgentFromFile(typeof(T)) is null
            ? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36"
            : GetUserAgentFromFile(typeof(T));
        var cookies = CookieParsers.GetFromFileRelativeToType(typeof(T));
        
        var handler = new HttpClientHandler();
        
        // Read cookies from file and load them into RestClientOptions
        foreach (var cookie in cookies)
        {
            handler.CookieContainer.Add(cookie);
        }

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(hostname)
        };

        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
        
        // Load custom header values
        if (customHeaders != null)
        {
            foreach (var header in customHeaders)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        return httpClient;
    }

    private static string? GetUserAgentFromFile(Type type)
    {
        var assemblyRoot = Path.GetDirectoryName(Assembly.GetAssembly(type)?.Location);
        if (string.IsNullOrEmpty(assemblyRoot))
        {
            return null;
        }

        var path = Path.Combine(assemblyRoot, "UA.txt");
        if (!File.Exists(path))
        {
            return null;
        }

        var file = File.ReadAllLines(path);
        return file.Length == 0 ? null : file[0];
    }
}
