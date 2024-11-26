using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using asuka.ProviderSdk;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Services.ProviderManager;

internal sealed class ProviderManager :  IProviderManager
{
    private readonly Dictionary<string, MetaInfo> _providers;
    private readonly ILogger<ProviderManager> _logger;

    public ProviderManager(ILogger<ProviderManager> logger)
    {
        _logger = logger;
        _providers = new Dictionary<string, MetaInfo>();
        
        // Look for existing providers in the Providers folder
        var providerRoot = Path.Combine(AppContext.BaseDirectory, "providers");
        if (!Directory.Exists(providerRoot))
        {
            Directory.CreateDirectory(providerRoot);
        }

        var providers = Directory.GetFiles(providerRoot, "asuka.Provider.*.dll", SearchOption.AllDirectories);
        foreach (var provider in providers)
        {
            var activatedInstance = TryLoadAssembly(provider);
            if (activatedInstance == null)
            {
                continue;
            }

            if (_providers.ContainsKey(activatedInstance.GetId()))
            {
                continue;
            }

            _providers.TryAdd(activatedInstance.GetId(), activatedInstance);
        }
    }

    private MetaInfo? TryLoadAssembly(string providerPath)
    {
        try
        {
            var types = Assembly.LoadFrom(providerPath).GetExportedTypes();
            var metaInfo = types
                .Single(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(MetaInfo)));

            return (MetaInfo)Activator.CreateInstance(metaInfo)!;
        }
        catch (Exception ex)
        {
            _logger.LogError("Loading of assembly failed due to an exception: {ex}", ex);
        }

        return null;
    }

    public MetaInfo? GetProviderForGalleryId(string galleryId)
    {
        foreach (var (_, provider) in _providers)
        {
            if (!provider.IsGallerySupported(galleryId))
            {
                continue;
            }

            _logger.LogInformation("Provider found for id: {galleryId} -> {providerId}", galleryId, provider.GetId());
            return provider;
        }

        _logger.LogInformation("No provider found for id: {galleryId}", galleryId);
        return null;
    }

    public MetaInfo? GetProviderByAlias(string alias)
    {
        foreach (var (key, provider) in _providers)
        {
            if (alias == key || provider.GetAliases().Contains(alias))
            {
                _logger.LogInformation("Provider alias matched: {alias} : {providerId}", alias, provider.GetId());
                return provider;
            }
        }

        _logger.LogInformation("No provider found with alias of {alias}", alias);
        return null;
    }

    public List<RegisteredProvider> GetAllRegisteredProviders()
    {
        return _providers.Values
            .Select(instance =>
            {
                return new RegisteredProvider
                {
                    Id = instance.GetId(),
                    Aliases = instance.GetAliases(),
                    Version = instance.GetVersion()
                };
            }).ToList();
    }
}
