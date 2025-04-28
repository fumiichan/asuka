using System.Text.Json;
using System.Text.RegularExpressions;
using asuka.Provider.Koharu.Api;
using asuka.Provider.Koharu.Contracts.Queries;
using asuka.Provider.Koharu.Extensions;
using asuka.Provider.Koharu.Mappers;
using asuka.Provider.Sdk;
using asuka.Provider.Sdk.Utilities;
using Refit;

namespace asuka.Provider.Koharu;

public sealed partial class Provider : MetaInfo
{
    private readonly IKoharuApi _api;

    private string _activeHost = string.Empty;
    private IKoharuImageApi? _imageApi;

    public Provider()
    {
        Id = "asuka.Provider.Koharu";
        Version = new Version(1, 0, 0, 2);
        ProviderAliases =
        [
            "koharu",
            "niyaniya",
            "shcale",
            "gehenna",
            "shupogaki",
            "hoshino",
            "seia"
        ];

        var apiClient = HttpClientFactory.CreateClientFromProvider<Provider>("https://api.niyaniya.moe/",
            new Dictionary<string, string>
            {
                { "Referer", "https://niyaniya.moe" },
                { "Origin", "https://niyaniya.moe" }
            });
        _api = RestService.For<IKoharuApi>(apiClient, new RefitSettings
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
        var regex = UrlRegex();
        return regex.IsMatch(galleryId);
    }

    public override async Task<Series> GetSeries(string galleryId, CancellationToken cancellationToken = default)
    {
        if (!IsGallerySupported(galleryId))
        {
            throw new NotSupportedException($"The supplied value'{galleryId}' is not supported by asuka.Providers.Koharu");
        }
        
        // Retrieve the required info from the URL
        var publicKey = PublicKeyRegex().Match(galleryId).Value;
        if (!int.TryParse(IdRegex().Match(galleryId).Value, out var id))
        {
            throw new NotSupportedException($"The supplied value'{galleryId}' is not supported by asuka.Providers.Koharu");
        }

        // Retrieve the gallery data
        var data = await _api.FetchSingle(id, publicKey, cancellationToken);

        var highestWidth = data.Data.Keys.FindHighestKey();
        var imageQueries = new ImageListQuery
        {
            Version = data.UpdatedAt,
            Width = int.Parse(highestWidth),
        };
        
        var images = await _api.FetchContents(
            id,
            publicKey,

            data.Data[highestWidth].Id,
            data.Data[highestWidth].PublicKey,
            
            imageQueries,
            cancellationToken);
        
        return data.ToSeries(images, imageQueries.Width);
    }

    public override Task<List<Series>> Search(SearchQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public override async Task<Series> GetRandom(CancellationToken cancellationToken = default)
    {
        var random = await _api.FetchRandom(cancellationToken);

        var gallery = await _api.FetchSingle(random.Id, random.PublicKey, cancellationToken);
        var highestWidth = gallery.Data.Keys.FindHighestKey();
        var imageQueries = new ImageListQuery
        {
            Version = gallery.UpdatedAt,
            Width = int.Parse(highestWidth),
        };
        
        var images = await _api.FetchContents(
            random.Id,
            random.PublicKey,

            gallery.Data[highestWidth].Id,
            gallery.Data[highestWidth].PublicKey,
            
            imageQueries,
            cancellationToken);
        
        return gallery.ToSeries(images, imageQueries.Width);
    }

    public override Task<List<Series>> GetRecommendations(string galleryId, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public override async Task<byte[]> GetImage(ChapterImage image, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(image.RemotePath);
        if (uri.Authority != _activeHost || _imageApi == null)
        {
            _activeHost = uri.Authority;
            
            var client = HttpClientFactory.CreateClientFromProvider<Provider>($"https://{uri.Authority}/",
                new Dictionary<string, string>
                {
                    { "Referer", "https://niyaniya.moe" },
                    { "Origin", "https://niyaniya.moe" }
                });
            
            _imageApi = RestService.For<IKoharuImageApi>(client);
        }
        
        // It must need to be 7 in length, just to be safe.
        var parameters = uri.AbsolutePath.Split('/');
        if (parameters.Length != 7)
        {
            throw new ArgumentException($"The remote path may not be usable: {image.RemotePath}", nameof(image.RemotePath));
        }
        
        var widthFromQuery = ResolutionRegex().Match(uri.Query).Value;

        // Parameters
        var id = parameters[2];
        var publicKey = parameters[3];
        var hash1 = parameters[4];
        var hash2 = parameters[5];
        var file = parameters[6];

        var route = $"{id}/{publicKey}/{hash1}/{hash2}/{file}";
        var query = new ImageQuery { Width = int.Parse(widthFromQuery) };

        var data = await _imageApi.GetImage(route, query, cancellationToken);
        return await data.ReadAsByteArrayAsync(cancellationToken);
    }

    [GeneratedRegex(@"^https:\/\/niyaniya\.moe\/g\/\d{1,9}\/[a-fA-F0-9]+$")]
    private static partial Regex UrlRegex();
    
    [GeneratedRegex(@"([a-fA-F0-9])+$")]
    private static partial Regex PublicKeyRegex();
    
    [GeneratedRegex(@"\d{1,9}")]
    private static partial Regex IdRegex();

    [GeneratedRegex(@"(\d{3,4})$")]
    private static partial Regex ResolutionRegex();
}
