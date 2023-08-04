namespace asuka.Sdk.Providers.Identity;

public enum FeatureSets
{
    /// <summary>
    /// This feature flag tells the consumer that it supports fetching of metadata
    /// such as searching, fetching single gallery.
    /// </summary>
    Fetch,
    
    /// <summary>
    /// This feature flag that it supports recommending galleries.
    /// </summary>
    Recommend,
    
    /// <summary>
    /// This feature flag tells the consumer that it supports picking random gallery.
    /// </summary>
    Random
}
