using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using asuka.Provider.Nhentai.Api;
using asuka.Provider.Nhentai.Api.Requests;
using asuka.Provider.Nhentai.Mappers;
using asuka.ProviderSdk;
using Refit;

namespace asuka.Provider.Nhentai;

public sealed partial class Provider : MetaInfo
{
    private readonly IGalleryApi _gallery;
    private readonly IGalleryImage _galleryImage;

    public Provider()
    {
        Id = "asuka.provider.nhentai";
        Version = new Version(1, 1, 0, 0);
        ProviderAliases =
        [
            "nh",
            "nhentai"
        ];

        // Configure request
        _gallery = RestService.For<IGalleryApi>(CreateHttpClient("https://nhentai.net/"), new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        });

        _galleryImage = RestService.For<IGalleryImage>(CreateHttpClient("https://i.nhentai.net/"));
    }

    public override bool IsGallerySupported(string galleryId)
    {
        var allowedInput1 = FullUrlRegex();
        var allowedInput2 = NumericOnlyRegex();

        return allowedInput1.IsMatch(galleryId) || allowedInput2.IsMatch(galleryId);
    }

    public override async Task<Series> GetSeries(string galleryId, CancellationToken cancellationToken = default)
    {
        // Sanity check
        if (!IsGallerySupported(galleryId))
        {
            throw new NotSupportedException($"The gallery ID supplied '{galleryId}' is not supported by asuka.Providers.Nhentai");
        }
        
        // Retrieve the code
        var codeRegex = CodeOnlyRegex();
        var code = codeRegex.Match(galleryId).Value;
        
        // Request
        var request = await _gallery.FetchSingle(code, cancellationToken);
        return request.ToSeries();
    }

    public override async Task<List<Series>> Search(SearchQuery query, CancellationToken cancellationToken = default)
    {
        var request = await _gallery.SearchGallery(new GallerySearchQuery
        {
            Queries = string.Join(" ", query.SearchQueries),
            PageNumber = query.PageNumber,
            Sort = query.Sort ?? "popularity"
        }, cancellationToken);

        return request.Result
            .Select(x => x.ToSeries())
            .ToList();
    }

    public override async Task<Series> GetRandom(CancellationToken cancellationToken = default)
    {
        var id = RandomNumberGenerator.GetInt32(1, 500_000);
        return await GetSeries(id.ToString(), cancellationToken);
    }

    public override async Task<List<Series>> GetRecommendations(string galleryId, CancellationToken cancellationToken = default)
    {
        // Sanity check
        if (!IsGallerySupported(galleryId))
        {
            throw new NotSupportedException($"The gallery ID supplied '{galleryId}' is not supported by asuka.Providers.Nhentai");
        }
        
        // Retrieve the code
        var codeRegex = CodeOnlyRegex();
        var code = codeRegex.Match(galleryId).Value;

        var request = await _gallery.FetchRecommended(code, cancellationToken);
        return request.Result
            .Select(x => x.ToSeries())
            .ToList();
    }

    public override async Task<byte[]> GetImage(string remotePath, CancellationToken cancellationToken = default)
    {
        var pathArguments = remotePath.Split(",");
        if (pathArguments.Length != 2)
        {
            throw new Exception($"Unable to download due to malformed remote path. remotePath: {remotePath}");
        }
        
        var request = await _galleryImage.GetImage(pathArguments[0], pathArguments[1], cancellationToken);
        return await request.ReadAsByteArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Reads the User Agent from UA.txt file relative to the assembly path.
    /// </summary>
    /// <returns></returns>
    private static string? GetUserAgentFromFile()
    {
        var assemblyRoot = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Provider))?.Location);
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

    /// <summary>
    /// Reads cookies from file
    /// </summary>
    /// <returns></returns>
    private static List<Cookie> ReadCookiesFromFile()
    {
        var assemblyRoot = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Provider))?.Location);
        if (string.IsNullOrEmpty(assemblyRoot))
        {
            return [];
        }
        
        var path = Path.Combine(assemblyRoot, "cookies.txt");
        if (!File.Exists(path))
        {
            return [];
        }

        if (CookieParsers.TryParseJsonExport(path, out var jsonCookies))
        {
            return jsonCookies;
        }

        return CookieParsers.TryParseNetscapeNavigatorExport(path, out var navigatorCookies)
            ? navigatorCookies
            : [];
    }

    private HttpClient CreateHttpClient(string hostname)
    {
        var handler = new HttpClientHandler();
        
        // Read cookies from file and load them into RestClientOptions
        foreach (var cookie in ReadCookiesFromFile())
        {
            handler.CookieContainer.Add(cookie);
        }

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(hostname)
        };
        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(
            GetUserAgentFromFile() ?? $"asuka.Provider.Nhentai {Version.Major}.{Version.Minor}");

        return httpClient;
    }

    [GeneratedRegex(@"^http(s)?:\/\/(nhentai\.net)\b([//g]*)\b([\d]{1,6})\/?$")]
    private static partial Regex FullUrlRegex();

    [GeneratedRegex(@"^#?\d{1,6}$")]
    private static partial Regex NumericOnlyRegex();

    [GeneratedRegex(@"\d{1,6}")]
    private static partial Regex CodeOnlyRegex();
}
