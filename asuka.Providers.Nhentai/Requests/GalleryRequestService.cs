using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using asuka.Providers.Nhentai.Api;
using asuka.Providers.Nhentai.Api.Queries;
using asuka.Providers.Nhentai.Configuration;
using asuka.Providers.Nhentai.Contracts;
using asuka.Sdk.Providers.Models;
using asuka.Sdk.Providers.Requests;
using Microsoft.Extensions.Logging;
using Refit;

namespace asuka.Providers.Nhentai.Requests;

public class GalleryRequestService : IGalleryRequestService
{
    private readonly ILogger<GalleryRequestService> _logger;
    private readonly IGalleryApi _api;
    private int _totalGalleryCountCache;

    public GalleryRequestService(HttpClient client, ILogger<GalleryRequestService> logger)
    {
        _logger = logger;
        _api = RestService.For<IGalleryApi>(client, new RefitSettings
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
        _logger.LogInformation("Code parsed on FetchSingle: {Code}", code);

        if (string.IsNullOrEmpty(id))
        {
            _logger.LogError("No result found on {Code} that matches requirement", code);
            return null;
        }

        try
        {
            var result = await _api.FetchSingle(id)
                .ConfigureAwait(false);
            _logger.LogInformation("API fetch success: {@Result}", result);
            return result.ToGalleryResult();
        }
        catch (Exception e)
        {
            _logger.LogError("API fetch fail. {@Exception}", e);
            return null;
        }
    }

    public async Task<IReadOnlyList<GalleryResult>> FetchRecommended(string code)
    {
        // Get code from URL
        var id = GetCode(code);
        _logger.LogInformation("Code parsed on FetchSingle: {Code}", code);

        if (string.IsNullOrEmpty(id))
        {
            _logger.LogError("No result found on {Code} that matches requirement", code);
            return null;
        }

        try
        {
            _logger.LogInformation("Attempting to get recommended response on ID: {Id}", id);
            var result = await _api.FetchRecommended(id)
                .ConfigureAwait(false);
            _logger.LogInformation("API fetch success: {@Result}", result);
            return result.Result.Select(x => x.ToGalleryResult()).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError("API fetch fail: {@Exception}", e);
            return null;
        }
    }

    public async Task<IReadOnlyList<GalleryResult>> Search(string query, string sort, int pageNumber)
    {
        var queries = new SearchQuery
        {
            Queries = $"{query}",
            PageNumber = pageNumber,
            Sort = sort
        };
        _logger.LogInformation("Search query: {@Queries}", queries);

        try
        {
            var result = await _api.SearchGallery(queries)
                .ConfigureAwait(false);
            _logger.LogInformation("API fetch success: {@Result}", result);
            return result.Result.Select(x => x.ToGalleryResult()).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError("API fetch fail: {@Exception}", e);
            return null;
        }
    }

    public async Task<GalleryResult> GetRandom()
    {
        _logger.LogInformation("Cached total gallery count is: {Count}", _totalGalleryCountCache);
        if (_totalGalleryCountCache == 0)
        {
            try
            {
                var result = await _api.FetchAll()
                    .ConfigureAwait(false);
                _totalGalleryCountCache = result.Result[0].Id;
                _logger.LogInformation("Total gallery count fetch and set to: {Count}", _totalGalleryCountCache);
            }
            catch (Exception e)
            {
                _logger.LogError("Unable to fetch total gallery count: {@Exception}", e);
                _totalGalleryCountCache = 400000;
                _logger.LogWarning("Setting total gallery count cache to {Count}", _totalGalleryCountCache);
            }
        }

        var id = RandomNumberGenerator.GetInt32(1, _totalGalleryCountCache);
        _logger.LogInformation("ID picked on random generator: {Id}", id);
        return await FetchSingle($"https://nhentai.net/g/{id}");
    }

    private string GetCode(string input)
    {
        var urlRegex = new Regex(@"^(https?)://nhentai.net/g/\d{1,6}/?$");
        if (urlRegex.IsMatch(input))
        {
            var codeRegex = new Regex(@"\d{1,6}");
            return codeRegex.Match(input).Value;
        }

        var codeOnlyRegex = new Regex(@"^\d{1,6}$");
        return codeOnlyRegex.IsMatch(input) ? codeOnlyRegex.Match(input).Value : string.Empty;
    }
}
