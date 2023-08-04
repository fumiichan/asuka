using System.Text.RegularExpressions;
using asuka.Providers.Nhentai.Api;
using asuka.Providers.Nhentai.Configuration;
using asuka.Sdk.Providers.Requests;
using Microsoft.Extensions.Logging;
using Refit;

namespace asuka.Providers.Nhentai.Requests;

public class GalleryImageRequestService : IGalleryImageRequestService
{
    private readonly IGalleryImage _api;
    private readonly ILogger<GalleryImageRequestService> _logger;

    public GalleryImageRequestService(HttpClient client, ILogger<GalleryImageRequestService> logger)
    {
        _logger = logger;
        _api = RestService.For<IGalleryImage>(client);
    }

    public async Task<HttpContent> FetchImage(string path)
    {
        _logger.LogInformation("FetchImage on GetGalleryImageRequestService got: {Path}", path);
        
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
}
