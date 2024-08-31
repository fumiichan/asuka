namespace asuka.ProviderSdk;

public abstract class MetaInfo
{
    /// <summary>
    /// Unique Identifier of the Provider
    /// </summary>
    /// <remarks>
    /// Conflicting IDs will be ignored if one is registered first. Whichever is loaded first (Note that loading or
    /// provider plugins are not predictable) will be loaded while the rest with the same IDs will be ignored.
    /// </remarks>
    protected string Id { get; init; } = "";
    
    /// <summary>
    /// Version of the Provider
    /// </summary>
    protected Version Version = new(1, 0, 0, 0);

    /// <summary>
    /// Supported aliases that can be used to specify a provider.
    /// </summary>
    /// <remarks>
    /// Ensure that the aliases are unique. If aliases are the same, the first one will be resolved. The order of which
    /// the provider are loaded are not guaranteed.
    /// </remarks>
    protected List<string> ProviderAliases { get; init; } = [];

    public string GetId() => Id;
    public Version GetVersion() => Version;
    public List<string> GetAliases() => ProviderAliases;

    /// <summary>
    /// Checks if the gallery is supported to use this provider
    /// </summary>
    /// <param name="galleryId"></param>
    /// <returns></returns>
    public abstract bool IsGallerySupported(string galleryId);

    /// <summary>
    /// Retrieves the series/gallery information
    /// </summary>
    /// <param name="galleryId">The full URL or ID of the gallery</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">The Provided Gallery ID is not supported by the provider</exception>
    /// <exception cref="Exception"></exception>
    public abstract Task<Series> GetSeries(string galleryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search something in the provider
    /// </summary>
    /// <remarks>
    /// Note that some queries may not support exclusions and other queries such as page ranges or
    /// date ranges. It may vary across providers.
    /// </remarks>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of galleries matches the query</returns>
    /// <exception cref="Exception"></exception>
    public abstract Task<List<Series>> Search(SearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a random gallery
    /// </summary>
    /// <remarks>
    /// Some providers don't support a random. It is possible to develop if the gallery IDs are just
    /// incrementing integers but for cases with GUIDs or strings as gallery IDs are not possible unless
    /// if the provider has API to randomly pick a gallery for you. Providers that doesn't support random
    /// can throw <see cref="NotSupportedException" /> if not supported.
    /// </remarks>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Unsupported by the provider</exception>
    public abstract Task<Series> GetRandom(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of galleries/series recommendation from gallery ID.
    /// </summary>
    /// <remarks>
    /// Some providers don't support recommendations and its impossible/very difficult to implement it
    /// on their own. Providers will throw <see cref="NotSupportedException" /> if not supported.
    /// </remarks>
    /// <param name="galleryId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns a list of galleries.</returns>
    /// <exception cref="NotSupportedException">Unsupported by the provider</exception>
    public abstract Task<List<Series>> GetRecommendations(string galleryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the image from the provider
    /// </summary>
    /// <param name="remotePath">Path to the resource.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns entire file in form of byte array</returns>
    public abstract Task<byte[]> GetImage(string remotePath, CancellationToken cancellationToken = default);
}
