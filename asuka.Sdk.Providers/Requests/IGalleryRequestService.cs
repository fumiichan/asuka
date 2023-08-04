using asuka.Sdk.Providers.Models;

namespace asuka.Sdk.Providers.Requests;

public interface IGalleryRequestService
{
    /// <summary>
    /// Fetches gallery from ID.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Task<GalleryResult> FetchSingle(string code);
    
    /// <summary>
    /// Fetches recommended galleries from gallery
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Task<IReadOnlyList<GalleryResult>> FetchRecommended(string code);
    
    /// <summary>
    /// Searches entire gallery from Query
    /// </summary>
    /// <param name="query"></param>
    /// <param name="sort"></param>
    /// <param name="pageNumber"></param>
    /// <returns></returns>
    Task<IReadOnlyList<GalleryResult>> Search(string query, string sort, int pageNumber);

    /// <summary>
    /// Fetch a Random Gallery
    /// </summary>
    /// <returns></returns>
    Task<GalleryResult> GetRandom();
}
