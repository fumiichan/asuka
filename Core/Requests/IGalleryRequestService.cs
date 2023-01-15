using System.Collections.Generic;
using System.Threading.Tasks;
using asuka.Core.Api.Queries;
using asuka.Core.Models;

namespace asuka.Core.Requests;

public interface IGalleryRequestService
{
    Task<GalleryResult> FetchSingleAsync(string url);
    Task<IReadOnlyList<GalleryResult>> FetchRecommendedAsync(string url);
    Task<IReadOnlyList<GalleryResult>> SearchAsync(SearchQuery query);
    Task<int> GetTotalGalleryCountAsync();
}
