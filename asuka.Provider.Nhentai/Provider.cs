using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using asuka.Provider.Nhentai.Contracts;
using asuka.Provider.Nhentai.Mappers;
using asuka.ProviderSdk;
using RestSharp;

namespace asuka.Provider.Nhentai;

public sealed partial class Provider : MetaInfo
{
    private readonly RestClientOptions _clientOptions;
    private readonly RestClientOptions _imageClientOptions;

    public Provider()
    {
        Id = "asuka.provider.nhentai";
        Version = new Version(1, 0, 0, 0);
        ProviderAliases =
        [
            "nh",
            "nhentai"
        ];

        // Configure request
        _clientOptions = new RestClientOptions("https://nhentai.net/")
        {
            ThrowOnAnyError = true,
            UserAgent = GetUserAgentFromFile() ?? $"asuka.Provider.Nhentai {Version.Major}.{Version.Minor}",
            CookieContainer = new CookieContainer()
        };

        _imageClientOptions = new RestClientOptions("https://i.nhentai.net/")
        {
            ThrowOnAnyError = true,
            UserAgent = GetUserAgentFromFile() ?? $"asuka.Provider.Nhentai {Version.Major}.{Version.Minor}",
            CookieContainer = new CookieContainer()
        };

        // Read cookies from file and load them into RestClientOptions
        foreach (var cookie in ReadCookiesFromFile())
        {
            _clientOptions.CookieContainer.Add(cookie);
            _imageClientOptions.CookieContainer.Add(cookie);
        }
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
        var client = new RestClient(_clientOptions);
        var request = new RestRequest($"/api/gallery/{code}");

        var response = await client.GetAsync<GalleryResponse>(request, cancellationToken);
        if (response == null)
        {
            throw new Exception("Failed to deserialize request.");
        }
        return response.ToSeries();
    }

    public override async Task<List<Series>> Search(SearchQuery query, CancellationToken cancellationToken = default)
    {
        var client = new RestClient(_clientOptions);
        var request = new RestRequest("/api/galleries/search");

        request.AddParameter("query", string.Join(" ", query.SearchQueries));
        request.AddParameter("page", query.PageNumber);
        request.AddParameter("sort", query.Sort);

        var response = await client.GetAsync<GallerySearchResponse>(request, cancellationToken);
        if (response?.Result == null)
        {
            throw new Exception($"Unable to retrieve search results. Request URL: {client.BuildUri(request).ToString()}");
        }

        return response.Result
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

        var client = new RestClient(_clientOptions);
        var request = new RestRequest($"/api/gallery/{code}/related");

        var response = await client.GetAsync<GalleryListResponse>(request, cancellationToken);
        if (response == null)
        {
            throw new Exception("Unable to fetch recommendations");
        }

        return response.Result
            .Select(x => x.ToSeries())
            .ToList();
    }

    public override async Task<byte[]> GetImage(string remotePath, CancellationToken cancellationToken = default)
    {
        var client = new RestClient(_imageClientOptions);
        var request = new RestRequest(remotePath);

        var data = await client.DownloadDataAsync(request, cancellationToken);
        if (data == null)
        {
            throw new Exception("Unable to download file.");
        }
        return data;
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

    [GeneratedRegex(@"^http(s)?:\/\/(nhentai\.net)\b([//g]*)\b([\d]{1,6})\/?$")]
    private static partial Regex FullUrlRegex();

    [GeneratedRegex(@"^#?\d{1,6}$")]
    private static partial Regex NumericOnlyRegex();

    [GeneratedRegex(@"\d{1,6}")]
    private static partial Regex CodeOnlyRegex();
}
