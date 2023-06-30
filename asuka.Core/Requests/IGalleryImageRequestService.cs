namespace asuka.Core.Requests;

public interface IGalleryImageRequestService
{
    /// <summary>
    /// Fetch images from URL path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Task<HttpContent> FetchImage(string path);
    
    /// <summary>
    /// Determines which provider is this belongs to.
    /// </summary>
    /// <returns></returns>
    ProviderData ProviderFor();
}
