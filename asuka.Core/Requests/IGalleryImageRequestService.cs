namespace asuka.Core.Requests;

public interface IGalleryImageRequestService : IGalleryImageProvidable
{
    /// <summary>
    /// Fetch images from URL path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Task<HttpContent> FetchImage(string path);
}
