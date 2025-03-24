using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using asuka.Provider.Nhentai.Api;
using asuka.Provider.Nhentai.Api.Requests;
using asuka.Provider.Nhentai.Mappers;
using asuka.Provider.Sdk;
using asuka.Provider.Sdk.Utilities;
using Refit;

namespace asuka.Provider.Nhentai;

public sealed partial class Provider : MetaInfo
{
    private readonly IGalleryApi _gallery;

    private int _activeHostnameIndex = 0;
    private IGalleryImage? _galleryImage;
    private readonly List<string> _knownHostnames = [
        "https://i1.nhentai.net",
        "https://i2.nhentai.net",
        "https://i3.nhentai.net",
        "https://i4.nhentai.net",
        "https://i5.nhentai.net",
        "https://i6.nhentai.net",
    ];

    public Provider()
    {
        Id = "asuka.provider.nhentai";
        Version = new Version(1, 1, 0, 2);
        ProviderAliases =
        [
            "nh",
            "nhentai"
        ];

        // Configure request
        var galleryClient = HttpClientFactory.CreateClientFromProvider<Provider>("https://nhentai.net/");
        _gallery = RestService.For<IGalleryApi>(galleryClient, new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        });
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
        return await TryGetImage(remotePath, cancellationToken: cancellationToken);
    }

    private async Task<byte[]> TryGetImage(string remotePath, int maxCalls = 0, CancellationToken cancellationToken = default)
    {
        // Check if the instance is null
        if (_galleryImage == null)
        {
            var client = HttpClientFactory.CreateClientFromProvider<Provider>(_knownHostnames[_activeHostnameIndex]);
            _galleryImage = RestService.For<IGalleryImage>(client);
        }
        
        var pathArguments = remotePath.Split("/");
        var mediaId = pathArguments[2];
        var filename = pathArguments[3];

        try
        {
            var response = await _galleryImage.GetImage(mediaId, filename, cancellationToken);
            return await response.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound && maxCalls < _knownHostnames.Count)
            {
                _activeHostnameIndex = (_activeHostnameIndex + 1) >= _knownHostnames.Count ? 0 : _activeHostnameIndex + 1;
                
                // Override the gallery instance
                var client = HttpClientFactory.CreateClientFromProvider<Provider>(_knownHostnames[_activeHostnameIndex]);
                _galleryImage = RestService.For<IGalleryImage>(client);
                
                return await TryGetImage(remotePath, maxCalls + 1, cancellationToken);
            }

            throw;
        }
    }

    [GeneratedRegex(@"^http(s)?:\/\/(nhentai\.net)\b([//g]*)\b([\d]{1,6})\/?$")]
    private static partial Regex FullUrlRegex();

    [GeneratedRegex(@"^#?\d{1,6}$")]
    private static partial Regex NumericOnlyRegex();

    [GeneratedRegex(@"\d{1,6}")]
    private static partial Regex CodeOnlyRegex();
}
