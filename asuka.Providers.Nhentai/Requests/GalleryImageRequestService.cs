using System.Text.RegularExpressions;
using asuka.Core.Requests;
using asuka.Providers.Nhentai.Api;
using asuka.Providers.Nhentai.Configuration;
using Refit;

namespace asuka.Providers.Nhentai.Requests;

public class GalleryImageRequestService : IGalleryImageRequestService
{
    private readonly IGalleryImage _api;

    public GalleryImageRequestService()
    {
        var handler = new HttpClientHandler();
        var config = OverrideConfigurations.GetConfiguration();

        foreach (var cookie in CookieConfiguration.LoadCookies())
        {
            handler.CookieContainer.Add(cookie);
        }

        var httpClient = new HttpClient(handler)
        {
            // There's other image hosts for this provider.
            // TODO add support for switching between image hosts
            BaseAddress = new Uri(config.ImageHostname)
        };
        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(config.UserAgent);

        _api = RestService.For<IGalleryImage>(httpClient);
    }

    public async Task<HttpContent> FetchImage(string path)
    {
        var regex = new Regex(@".(\d+\/\d+.(jpg|png|gif))$");
        if (!regex.IsMatch(path))
        {
            return null;
        }

        var mediaIdRegex = new Regex(@"\d{2,}");
        var mediaId = mediaIdRegex.Match(path).Value;

        var pageIdRegex = new Regex(@"\d+.(jpg|png|gif)");
        var fileName = pageIdRegex.Match(path).Value;

        return await _api.GetImage(mediaId, fileName);
    }
    
    public string ProviderFor()
    {
        return "nhentai";
    }
}
