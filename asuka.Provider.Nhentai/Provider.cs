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
    private readonly GalleryImageProvider _clients = new();

    public Provider()
    {
        Id = "asuka.provider.nhentai";
        Version = new Version(1, 2, 0, 4);
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
        await WaitOnJitter(cancellationToken);
        
        return request.ToSeries();
    }

    public override async Task<SearchInfo> Search(SearchQuery query, CancellationToken cancellationToken = default)
    {
        var request = await _gallery.SearchGallery(new GallerySearchQuery
        {
            Queries = string.Join(" ", query.SearchQueries),
            PageNumber = query.PageNumber,
            Sort = query.Sort ?? "popular"
        }, cancellationToken);

        return new()
        {
            Result = request.Result
                .Select(x => new SearchResultObject
                {
                    Id = x.Id.ToString(),
                    Title = string.IsNullOrEmpty(x.JapaneseTitle) ? x.EnglishTitle : x.JapaneseTitle
                })
                .ToList(),
            NumberOfPages = request.NumPages,
            TotalPages = request.Total,
        };
    }

    public override async Task<Series> GetRandom(CancellationToken cancellationToken = default)
    {
        var id = RandomNumberGenerator.GetInt32(1, 500_000);
        return await GetSeries(id.ToString(), cancellationToken);
    }

    public override async Task<SearchInfo> GetRecommendations(string galleryId, CancellationToken cancellationToken = default)
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
        return new()
        {
            Result = request.Result
                .Select(x => new SearchResultObject
                {
                    Id = x.Id.ToString(),
                    Title = string.IsNullOrEmpty(x.JapaneseTitle) ? x.EnglishTitle : x.JapaneseTitle
                })
                .ToList(),
            NumberOfPages = request.NumPages,
            TotalPages = request.Total,
        };
    }

    public override async Task<byte[]> GetImage(ChapterImage image, CancellationToken cancellationToken = default)
    {
        return await TryGetImage(image, cancellationToken: cancellationToken);
    }

    private async Task<byte[]> TryGetImage(ChapterImage image, CancellationToken cancellationToken = default)
    {
        var clients = await _clients.GetImageProviders(_gallery);
        var retries = 0;
        
        // Keep track of which CDN works.
        // Starts with 1 to skip the i1 domain, which is known to have issues with newer galleries.
        var goodIndex = 1;

        while (retries < clients.Count)
        {
            try
            {
                var response = await clients[goodIndex].Client
                    .GetImage(image.RemotePath, cancellationToken);
                var data = await response.ReadAsByteArrayAsync(cancellationToken);
                
                await WaitOnJitter(cancellationToken);

                return data;
            }
            catch (OperationCanceledException) { throw; }
            catch
            {
                goodIndex = (goodIndex + 1) % clients.Count;
                retries++;
                
                // Sleep
                var baseDelay = (int)Math.Pow(2, retries) * 1000;
                var backoffJitter = RandomNumberGenerator.GetInt32(0, 1000);
                var totalDelay = baseDelay + backoffJitter;
                
                await Task.Delay(totalDelay, cancellationToken);
            }
        }
        
        // Throw when it fails
        throw new Exception($"Unable to download image after {retries} retries: {image.RemotePath}");
    }

    private async Task WaitOnJitter(CancellationToken cancellationToken = default)
    {
        var successJitter = RandomNumberGenerator.GetInt32(50, 250); 
        await Task.Delay(successJitter, cancellationToken);
    }

    [GeneratedRegex(@"^http(s)?:\/\/(nhentai\.net)\b([//g]*)\b([\d]{1,6})\/?$")]
    private static partial Regex FullUrlRegex();

    [GeneratedRegex(@"^#?\d{1,6}$")]
    private static partial Regex NumericOnlyRegex();

    [GeneratedRegex(@"\d{1,6}")]
    private static partial Regex CodeOnlyRegex();
}
