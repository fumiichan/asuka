using System.Text.RegularExpressions;
using asuka.Core.Requests;
using asuka.Providers.Nhentai.Api;
using asuka.Providers.Nhentai.Configuration;
using Microsoft.Extensions.Logging;
using Refit;

namespace asuka.Providers.Nhentai.Requests;

public class GalleryImageRequestService : IGalleryImageRequestService
{
    private IGalleryImage _api;
    private readonly ILogger _logger;

    public GalleryImageRequestService(ILogger logger)
    {
        _logger = logger;
        Initialize();
    }

    private void Initialize()
    {
        var handler = new HttpClientHandler();
        var config = OverrideConfigurations.GetConfiguration();

        foreach (var cookie in CookieConfiguration.LoadCookies())
        {
            _logger.LogInformation("Loaded cookie into image request service {@Cookie}", cookie);
            handler.CookieContainer.Add(cookie);
        }

        var httpClient = new HttpClient(handler)
        {
            // There's other image hosts for this provider.
            BaseAddress = new Uri(config.ImageHostname)
        };
        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(config.UserAgent);
        _logger.LogInformation("UserAgent loaded on GalleryImageRequestService: {@UserAgentHttpClient}", httpClient.DefaultRequestHeaders.UserAgent);


        _api = RestService.For<IGalleryImage>(httpClient);
    }

    public async Task<HttpContent> FetchImage(string path)
    {
        _logger.LogInformation("FetchImage on GalleryImageRequestService got: {Path}", path);
        
        var regex = new Regex(@".(\d+\/\d+.(jpg|png|gif))$");
        if (!regex.IsMatch(path))
        {
            _logger.LogError("{Path} doesn't match the requirement for image download!", path);
            return null;
        }

        var mediaIdRegex = new Regex(@"\d{2,}");
        var mediaId = mediaIdRegex.Match(path).Value;
        _logger.LogInformation("MediaID: {Media}", mediaId);

        var pageIdRegex = new Regex(@"\d+.(jpg|png|gif)");
        var fileName = pageIdRegex.Match(path).Value;
        _logger.LogInformation("FileName: {Filename}", fileName);

        try
        {
            var result = await _api.GetImage(mediaId, fileName);
            _logger.LogInformation("Successfully downloaded the image: {Path}", path);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError("Unable to download image due to an exception: {@Exception}", e);
            return null;
        }
    }
    
    public ProviderData ProviderFor()
    {
        return new ProviderData
        {
            For = "nhentai",
            Base = "https://nhentai.net"
        };
    }
}
