using System.Collections.Generic;
using System.Linq;
using asuka.Core.Requests;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Services;

public class Provider
{
    public IGalleryImageRequestService ImageApi { get; init; }
    public IGalleryRequestService Api { get; init; }
}

public class ProviderResolverService
{
    private readonly IEnumerable<IGalleryRequestService> _apis;
    private readonly IEnumerable<IGalleryImageRequestService> _imageApis;
    private readonly ILogger _logger;
    
    public ProviderResolverService(
        IEnumerable<IGalleryRequestService> apis,
        IEnumerable<IGalleryImageRequestService> imageApis,
        ILogger logger)
    {
        _apis = apis;
        _imageApis = imageApis;
        _logger = logger;
    }

    public Provider GetProviderByUrl(string url)
    {
        var result = _apis
            .FirstOrDefault(x => url.StartsWith(x.ProviderFor().Base));

        // Main API provider for this cannot be found. We can safely assume that the image provider doesn't exist too.
        if (result is null)
        {
            _logger.LogError("Provider cannot be found by URL: {Url}", url);
            return null;
        }
        
        _logger.LogInformation("Provider for Api by URL found: {Url} = {Provider}", url, result.ProviderFor().For);

        var imageApi = _imageApis
            .FirstOrDefault(x => x.ProviderFor().For == result.ProviderFor().For);

        // This might be a problem for certain third-party providers where they don't provide modules for downloading
        // the actual images. Returning null makes sense because the provider lacks functionality that this application
        // is made for.
        if (imageApi is null)
        {
            _logger.LogError("Image provider cannot be found by URL: {Url}", url);
            return null;
        }
        
        _logger.LogInformation("Provider for image by URL found: {Url} = {Provider}", url, result.ProviderFor().For);

        return new Provider
        {
            ImageApi = imageApi,
            Api = result
        };
    }

    public Provider GetProviderByName(string name)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
        {
            _logger.LogError("Unable to get provider. Provider: {Name}", name);
            return null;
        }

        var result = _apis
            .FirstOrDefault(x => x.ProviderFor().For == name);

        if (result is null)
        {
            _logger.LogError("Unable to get api provider. Provider: {Name}", name);
            return null;
        }
        
        _logger.LogInformation("Provider api found for: {Name}", name);

        var imageApi = _imageApis
            .FirstOrDefault(x => x.ProviderFor().For == name);

        if (imageApi is null)
        {
            _logger.LogError("Unable to get provider. Provider: {Name}", name);
            return null;
        }
        
        _logger.LogInformation("Provider image api found for: {Name}", name);

        return new Provider
        {
            ImageApi = imageApi,
            Api = result
        };
    }
}
