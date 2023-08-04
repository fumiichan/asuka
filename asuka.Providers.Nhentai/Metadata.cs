using asuka.Providers.Nhentai.Configuration;
using asuka.Providers.Nhentai.Requests;
using asuka.Sdk.Providers.Identity;
using asuka.Sdk.Providers.Requests;
using Microsoft.Extensions.Logging;

namespace asuka.Providers.Nhentai;

public sealed class Metadata : ProviderMetadata
{
    private readonly IGalleryRequestService _galleryRequestService;
    private readonly IGalleryImageRequestService _galleryImageRequestService;
    private readonly ILogger<Metadata> _logger;
    
    public Metadata(
        ILogger<Metadata> logger,
        ILogger<GalleryRequestService> loggerGalleryRequestService,
        ILogger<GalleryImageRequestService> loggerImageRequestService)
    {
        // Setup Provider identity
        AddFeatureSet(
            FeatureSets.Fetch,
            FeatureSets.Random,
            FeatureSets.Recommend);
        SetId("nhentai");
        SetName("nhentai.net API");
        AddSupportedUrl(
            "https://nhentai.net/");

        // DI
        _logger = logger;

        // Request services
        var config = OverrideConfigurations.GetConfiguration();

        _galleryRequestService = new GalleryRequestService(
            GetHttpClient(config.ApiHostname, config.UserAgent), loggerGalleryRequestService);
        _galleryImageRequestService = new GalleryImageRequestService(
            GetHttpClient(config.ImageHostname, config.UserAgent), loggerImageRequestService);
    }

    private HttpClient GetHttpClient(string hostname, string userAgent)
    {
        var handler = new HttpClientHandler();

        foreach (var cookie in CookieConfiguration.LoadCookies())
        {
            _logger.LogInformation("Cookie loaded on GalleryRequestService: {@Cookie}", cookie);
            handler.CookieContainer.Add(cookie);
        }

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(hostname)
        };
        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
        _logger.LogInformation("UserAgent loaded on GalleryRequestService: {@UserAgentHttpClient}", httpClient.DefaultRequestHeaders.UserAgent);

        return httpClient;

    }

    public override IGalleryRequestService GetGalleryRequestService() => _galleryRequestService;
    public override IGalleryImageRequestService GetGalleryImageRequestService() => _galleryImageRequestService;
}
