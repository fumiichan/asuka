using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using asuka.Provider.Common;
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

        // Notice: This may be dynamic in the near future. There's domains such as i<n>.nhentai.net
        // This hints that maybe in the near future, some images will be served only on that domain and currently
        // the API we are using doesn't have that kind of detail.
        var imageClient = HttpClientFactory.CreateClientFromProvider<Provider>("https://i.nhentai.net/");
        _galleryImage = RestService.For<IGalleryImage>(imageClient);
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

    [GeneratedRegex(@"^http(s)?:\/\/(nhentai\.net)\b([//g]*)\b([\d]{1,6})\/?$")]
    private static partial Regex FullUrlRegex();

    [GeneratedRegex(@"^#?\d{1,6}$")]
    private static partial Regex NumericOnlyRegex();

    [GeneratedRegex(@"\d{1,6}")]
    private static partial Regex CodeOnlyRegex();
}
