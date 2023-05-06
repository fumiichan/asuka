using asuka.Core.Models;

namespace asuka.Core.Requests;

public interface IGalleryRequestService
{
    Task<GalleryResult> FetchSingleAsync(string url);
    Task<IReadOnlyList<GalleryResult>> FetchRecommendedAsync(string url);
    Task<IReadOnlyList<GalleryResult>> SearchAsync(string query, int page, string sort);
    Task<int> GetTotalGalleryCountAsync();
}
