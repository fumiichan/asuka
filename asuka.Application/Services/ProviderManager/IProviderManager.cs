using System;
using System.Collections.Generic;
using asuka.ProviderSdk;

namespace asuka.Application.Services.ProviderManager;

internal interface IProviderManager
{
    /// <summary>
    /// Gets the provider based on the gallery ID/URL provided.
    /// </summary>
    /// <param name="galleryId"></param>
    /// <returns>Returns the <see cref="MetaInfo" /> that supports it. Returns null if found none.</returns>
    MetaInfo? GetProviderForGalleryId(string galleryId);

    /// <summary>
    /// Gets the provider based on the alias.
    /// </summary>
    /// <param name="alias">The alias or the ID of the provider</param>
    /// <returns>Returns the <see cref="MetaInfo" /> that has that alias. Returns null if found none.</returns>
    MetaInfo? GetProviderByAlias(string alias);

    /// <summary>
    /// Gets the list of providers currently detected
    /// </summary>
    /// <returns></returns>
    List<RegisteredProvider> GetAllRegisteredProviders();
}

internal sealed class RegisteredProvider
{
    public required string Id { get; init; }
    public required Version Version { get; init; }
    public required List<string> Aliases { get; init; }
}

