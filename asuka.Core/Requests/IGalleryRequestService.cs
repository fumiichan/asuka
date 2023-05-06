using asuka.Api.Queries;
using asuka.Core.Models;

namespace asuka.Core.Requests;

public interface IGalleryRequestService
{
    Task<GalleryResult> FetchSingle(string url);
    Task<IReadOnlyList<GalleryResult>> FetchRecommended(string url);
    Task<IReadOnlyList<GalleryResult>> Search(SearchQuery query);
    Task<int> GetTotalGalleryCount();
}
