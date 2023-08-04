namespace asuka.Sdk.Providers.Requests;

public interface IGalleryImageRequestService
{
    /// <summary>
    /// Fetch images from URL path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Task<HttpContent> FetchImage(string path);
}
