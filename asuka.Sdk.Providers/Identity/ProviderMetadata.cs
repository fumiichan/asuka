using asuka.Sdk.Providers.Requests;

namespace asuka.Sdk.Providers.Identity;

public abstract class ProviderMetadata
{
    private readonly HashSet<FeatureSets> _featureSets = new();
    private readonly HashSet<string> _supportedUrls = new();
    private string _id = "";
    private string _name = "";

    /// <summary>
    /// Adds feature to the supported feature set.
    /// </summary>
    /// <param name="featureSets"></param>
    protected void AddFeatureSet(params FeatureSets[] featureSets)
    {
        foreach (var feature in featureSets)
        {
            _featureSets.Add(feature);
        }
    }

    /// <summary>
    /// Adds supported url that this provider can handle.
    /// </summary>
    /// <param name="baseUrls"></param>
    protected void AddSupportedUrl(params string[] baseUrls)
    {
        foreach (var url in baseUrls)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                continue;
            }

            _supportedUrls.Add(url);
        }
    }

    public abstract IGalleryRequestService GetGalleryRequestService();
    public abstract IGalleryImageRequestService GetGalleryImageRequestService();

    /// <summary>
    /// Configures the ID of the provider.
    /// </summary>
    /// <param name="id"></param>
    protected void SetId(string id) => _id = id;

    /// <summary>
    /// Configures the display name of the provider.
    /// </summary>
    /// <param name="name"></param>
    protected void SetName(string name) => _name = name;

    /// <summary>
    /// Checks if provider contains the feature if available.
    /// </summary>
    /// <param name="feature"></param>
    /// <returns></returns>
    public bool IsFeatureSupported(FeatureSets feature) => _featureSets.Contains(feature);

    /// <summary>
    /// Gets the ID of the provider.
    /// </summary>
    /// <returns></returns>
    public string GetId() => _id;

    /// <summary>
    /// Gets the name of the provider.
    /// </summary>
    /// <returns></returns>
    public string GetName() => _name;

    /// <summary>
    /// Checks if the URL is supported by the provider.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public bool IsUrlSupported(string url)
    {
        return !string.IsNullOrEmpty(_supportedUrls.FirstOrDefault(url.StartsWith));
    }
}
