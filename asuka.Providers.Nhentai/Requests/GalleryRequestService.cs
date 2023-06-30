using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using asuka.Core.Models;
using asuka.Core.Requests;
using asuka.Providers.Nhentai.Api;
using asuka.Providers.Nhentai.Api.Queries;
using asuka.Providers.Nhentai.Configuration;
using asuka.Providers.Nhentai.Contracts;
using Refit;

namespace asuka.Providers.Nhentai.Requests;

public class GalleryRequestService : IGalleryRequestService
{
    private readonly IGalleryApi _api;
    private readonly Regex _urlRegex = new Regex(@"^(https?):\/\/nhentai.net\/g\/\d{1,6}\/?$");
    private int _totalGalleryCountCache;

    public GalleryRequestService()
    {
        var handler = new HttpClientHandler();
        var config = OverrideConfigurations.GetConfiguration();

        foreach (var cookie in CookieConfiguration.LoadCookies())
        {
            handler.CookieContainer.Add(cookie);
        }

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(config.ApiHostname)
        };
        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(config.UserAgent);

        _api = RestService.For<IGalleryApi>(httpClient, new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        });
    }

    public async Task<GalleryResult> FetchSingle(string code)
    {
        // Get code from URL
        var id = GetCode(code);

        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException($"Invalid input: {code}");
        }

        var result = await _api.FetchSingle(id)
            .ConfigureAwait(false);
        return result.ToGalleryResult();
    }

    public async Task<IReadOnlyList<GalleryResult>> FetchRecommended(string code)
    {
        // Get code from URL
        var id = GetCode(code);

        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException($"Invalid input: {code}");
        }

        var result = await _api.FetchRecommended(id)
            .ConfigureAwait(false);
        return result.Result.Select(x => x.ToGalleryResult()).ToList();
    }

    public async Task<IReadOnlyList<GalleryResult>> Search(string query, string sort, int pageNumber)
    {
        var queries = new SearchQuery
        {
            Queries = $"{query}",
            PageNumber = pageNumber,
            Sort = sort
        };
        var result = await _api.SearchGallery(queries)
            .ConfigureAwait(false);
        return result.Result.Select(x => x.ToGalleryResult()).ToList();
    }

    public async Task<GalleryResult> GetRandom()
    {
        if (_totalGalleryCountCache == 0)
        {
            var result = await _api.FetchAll()
                .ConfigureAwait(false);
            _totalGalleryCountCache = result.Result[0].Id;
        }

        var id = RandomNumberGenerator.GetInt32(1, _totalGalleryCountCache);
        return await FetchSingle($"https://nhentai.net/g/{id}");
    }

    public ProviderData ProviderFor()
    {
        return new ProviderData
        {
            For = "nhentai",
            Base = "https://nhentai.net"
        };
    }

    public bool IsFullUrlValid(string url)
    {
        return _urlRegex.IsMatch(url);
    }

    private string GetCode(string input)
    {
        if (_urlRegex.IsMatch(input))
        {
            var codeRegex = new Regex(@"\d{1,6}");
            return codeRegex.Match(input).Value;
        }

        var codeOnlyRegex = new Regex(@"^\d{1,6}$");
        return codeOnlyRegex.IsMatch(input) ? codeOnlyRegex.Match(input).Value : string.Empty;
    }
}
