using System.Text.Json;
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
        var result = await _api.FetchSingle(code)
            .ConfigureAwait(false);
        return result.ToGalleryResult();
    }

    public async Task<IReadOnlyList<GalleryResult>> FetchRecommended(string code)
    {
        var result = await _api.FetchRecommended(code)
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

    public async Task<int> GetTotalGalleryCount()
    {
        var result = await _api.FetchAll()
            .ConfigureAwait(false);
        var id = result.Result[0].Id;

        return id;
    }

    public string ProviderFor()
    {
        return "nhentai";
    }
}
