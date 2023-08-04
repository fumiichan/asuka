using System.Collections.Generic;
using System.Linq;
using asuka.Sdk.Providers.Identity;
using asuka.Sdk.Providers.Requests;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Services;

public class Provider
{
    public IGalleryImageRequestService ImageApi { get; init; }
    public IGalleryRequestService Api { get; init; }
}

public class ProviderResolverService
{
    private readonly IEnumerable<ProviderMetadata> _apis;
    private readonly ILogger<ProviderResolverService> _logger;
    
    public ProviderResolverService(
        IEnumerable<ProviderMetadata> apis,
        ILogger<ProviderResolverService> logger)
    {
        _apis = apis;
        _logger = logger;
    }

    public Provider GetProviderByUrl(string url)
    {
        var result = _apis.FirstOrDefault(x => x.IsUrlSupported(url));

        // Main API provider for this cannot be found. We can safely assume that the image provider doesn't exist too.
        if (result is null)
        {
            _logger.LogError("Provider cannot be found by URL: {Url}", url);
            return null;
        }
        
        _logger.LogInformation("Provider for Api by URL found: {Url} = {Provider}", url, result.GetId());
        
        return new Provider
        {
            ImageApi = result.GetGalleryImageRequestService(),
            Api = result.GetGalleryRequestService()
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
            .FirstOrDefault(x => x.GetId() == name);

        if (result is null)
        {
            _logger.LogError("Unable to get api provider. Provider: {Name}", name);
            return null;
        }
        
        _logger.LogInformation("Provider api found for: {Name}", name);

        return new Provider
        {
            ImageApi = result.GetGalleryImageRequestService(),
            Api = result.GetGalleryRequestService()
        };
    }
}
