using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using asuka.Provider.Nhentai.Api;
using asuka.Provider.Nhentai.Api.Client;
using asuka.Provider.Nhentai.Api.Requests;
using asuka.Provider.Nhentai.Mappers;
using asuka.Provider.Sdk;
using asuka.Provider.Sdk.Utilities;
using Refit;

namespace asuka.Provider.Nhentai;

public sealed partial class Provider : MetaInfo
{
    private readonly IGalleryApi _gallery;

    private readonly List<ImageRequestClient<Provider>> _clients;
    private int _active;

    public Provider()
    {
        Id = "asuka.provider.nhentai";
        Version = new Version(1, 2, 0, 3);
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
        
        // Configure Image request
        _clients =
        [
            new ImageRequestClient<Provider>("https://i1.nhentai.net"),
            new ImageRequestClient<Provider>("https://i2.nhentai.net"),
            new ImageRequestClient<Provider>("https://i3.nhentai.net"),
            new ImageRequestClient<Provider>("https://i4.nhentai.net"),
            new ImageRequestClient<Provider>("https://i5.nhentai.net"),
            new ImageRequestClient<Provider>("https://i6.nhentai.net")
        ];
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

    public override async Task<byte[]> GetImage(ChapterImage image, CancellationToken cancellationToken = default)
    {
        return await TryGetImage(image, cancellationToken: cancellationToken);
    }

    private async Task<byte[]> TryGetImage(ChapterImage image, int retryCount = 0, CancellationToken cancellationToken = default)
    {
        var client = _clients[_active];

        try
        {
            var pathArguments = image.RemotePath.Split("/");
            var mediaId = pathArguments[2];
            var filename = pathArguments[3];
            
            var response = await client.Client.GetImage(mediaId, filename, cancellationToken);
            return await response.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            // Retry with a different host if it doesn't exist:
            if (ex.StatusCode == HttpStatusCode.NotFound && retryCount <= _clients.Count)
            {
                _active = (_active + 1) >= _clients.Count
                    ? 0
                    : _active + 1;
                
                return await TryGetImage(image, retryCount + 1, cancellationToken);
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
