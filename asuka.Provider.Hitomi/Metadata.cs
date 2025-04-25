using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using asuka.Provider.Hitomi.Contracts.Responses;
using asuka.Provider.Sdk;
using asuka.Provider.Sdk.Utilities;

namespace asuka.Provider.Hitomi;

public sealed partial class Metadata : MetaInfo
{
    public Metadata()
    {
        Id = "asuka.provider.hitomi";
        Version = new Version(1, 1, 0, 0);
        ProviderAliases =
        [
            "hitomi",
            "hitomi.la"
        ];
    }
    
    public override bool IsGallerySupported(string galleryId)
    {
        return !string.IsNullOrEmpty(galleryId) && (FullUrlRegex().IsMatch(galleryId) || GalleryIdRegex().IsMatch(galleryId));
    }

    public override async Task<Series> GetSeries(string galleryId, CancellationToken cancellationToken = default)
    {
        var client = HttpClientFactory.CreateClientFromProvider<Metadata>("https://ltn.gold-usergeneratedcontent.net",
            new Dictionary<string, string>
            {
                { "Referer", galleryId },
            });
        
        // Find ID from the URL.
        var id = GalleryIdRegex().Match(galleryId).Groups[1].Value;
        
        return await GetInfo(client, id, cancellationToken);
    }
    
    public override async Task<List<Series>> GetRecommendations(string galleryId, CancellationToken cancellationToken = default)
    {
        var id = GalleryIdRegex().Match(galleryId).Groups[1].Value;
        
        var client = HttpClientFactory.CreateClientFromProvider<Metadata>("https://ltn.gold-usergeneratedcontent.net",
            new Dictionary<string, string>
            {
                { "Referer", galleryId },
            });
        
        var ids = await GetRelativeIdsFromGalleryId(client, id, cancellationToken);
        
        var result = new List<Series>();
        foreach (var related in ids)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var info = await GetInfo(client, related.ToString(), cancellationToken);
            result.Add(info);
        }
        
        return result;
    }

    public override async Task<byte[]> GetImage(string remotePath, CancellationToken cancellationToken = default)
    {
        var @params = remotePath.Split('|');
        
        var uri = new Uri(@params[0]);
        var referer = @params[1];

        var client = HttpClientFactory.CreateClientFromProvider<Metadata>($"https://{uri.Authority}",
            new Dictionary<string, string>
            {
                { "Referer", referer },
            });
        var request = await client.GetAsync(uri.AbsolutePath, cancellationToken);
        request.EnsureSuccessStatusCode();
        
        return await request.Content.ReadAsByteArrayAsync(cancellationToken);
    }
    
    #region Unsupported Methods
    public override Task<List<Series>> Search(SearchQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public override Task<Series> GetRandom(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
    #endregion

    #region Helper Methods

    private async Task<IEnumerable<int>> GetRelativeIdsFromGalleryId(HttpClient client, string id,
        CancellationToken cancellationToken = default)
    {
        // Retrieve the Javascript file we need that contains metadata of the images.
        using var request = await client.GetAsync($"galleries/{id}.js", cancellationToken);
        request.EnsureSuccessStatusCode();

        // Ensuring that we get a proper json, we need to remove some stuff
        var response = await request.Content.ReadAsStringAsync(cancellationToken);
        var jsonData = response[response.IndexOf('{')..];
        
        var data = JsonSerializer.Deserialize<GalleryInformation>(jsonData);
        if (data is null)
        {
            throw new Exception("Parsing data failed.");
        }
        
        return data.Related ?? [];
    }
    
    private async Task<Series> GetInfo(HttpClient client, string id, CancellationToken cancellationToken = default)
    {
        // Retrieve the Javascript file we need that contains metadata of the images.
        using var request = await client.GetAsync($"galleries/{id}.js", cancellationToken);
        request.EnsureSuccessStatusCode();

        // Ensuring that we get a proper json, we need to remove some stuff
        var response = await request.Content.ReadAsStringAsync(cancellationToken);
        var jsonData = response[response.IndexOf('{')..];
        
        var data = JsonSerializer.Deserialize<GalleryInformation>(jsonData, new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        });
        if (data is null)
        {
            throw new Exception("Parsing data failed.");
        }
        
        // Get the image parameters somewhere
        var ggCode = await HitomiHelper.GetGgCode(client, cancellationToken);
        var referrer = $"https://hitomi.la/reader/{id}.html";
        
        // Constants for image url
        // Default to "w", other formats seems to fail for some reason.
        // Also force it to use webp instead.
        const string domainLetter = "w";
        const string ext = ".webp";
        
        var chapter = new Chapter
        {
            Id = 1,
            Pages = data.Files.Select(x =>
            {
                // https://{a}{b}.gold-usergeneratedcontent.net/{c}/{d}/{e}.{f}
                // Legend:
                //     a -> the first letter of the file extension    domainLetter
                //     b -> some digit from gg.js                     domainNumeric
                //     c -> some digits (probably an epoch time)      ggCode.B
                //     d -> some random 4 digits again                dictKey
                //     e -> hash of the file                          x.Hash
                //     f -> file extension                            ext (including dot)

                // Retrieve the key as index from the dictionary
                var key = x.Hash[^1] + x.Hash.Substring(x.Hash.Length - 3, 2);
                var dictKey = int.Parse(key, NumberStyles.HexNumber);
                var domainNumeric = (ggCode.M.TryGetValue(dictKey, out var code) ? code : ggCode.D) + 1;

                var url = $"https://{domainLetter}{domainNumeric}.gold-usergeneratedcontent.net" +
                          $"/{ggCode.B}" +
                          $"/{dictKey}" +
                          $"/{x.Hash}" +
                          ext;
                return new Chapter.ChapterImages
                {
                    Filename = x.Name.Replace(Path.GetExtension(x.Name), ext),
                    ImageRemotePath = url + "|" + referrer,
                };
            }).ToList()
        };

        return new Series
        {
            Title = string.IsNullOrEmpty(data.JapaneseTitle) ? data.Title : data.JapaneseTitle,
            Artists = data.Groups?.Select(x => x.Name).ToList() ?? [],
            Authors = [],
            Genres = data.MergeTags(),
            Chapters = [chapter]
        };
    }
    #endregion
    
    [GeneratedRegex(@"^https:\/{2}(hitomi.la)\/(doujinshi|imageset)\/(.+)-\d{1,9}\.html$")]
    private static partial Regex FullUrlRegex();

    [GeneratedRegex(@"([0-9]+)(?:\.html)?$")]
    private static partial Regex GalleryIdRegex();
}
