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
        var assemblyPath = Assembly.GetAssembly(typeof(Program))!.Location;
        var assemblyRootPath = Path.GetDirectoryName(assemblyPath);
        var providerRoot = Path.Combine(assemblyRootPath!, "providers");

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

    public Dictionary<string, Version> GetAllRegisteredProviders()
    {
        var dict = new Dictionary<string, Version>();
        foreach (var (key, instance) in _providers)
        {
            dict.Add(key, instance.GetVersion());
        }

        return dict;
    }
}
