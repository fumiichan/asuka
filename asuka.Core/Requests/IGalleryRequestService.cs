using asuka.Core.Models;

namespace asuka.Core.Requests;

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
    /// Gets total gallery count.
    /// </summary>
    /// <returns></returns>
    Task<int> GetTotalGalleryCount();
    
    /// <summary>
    /// Determines which provider is this belongs to.
    /// </summary>
    /// <returns></returns>
    string ProviderFor();
}
