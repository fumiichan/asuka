using System.Threading.Tasks;
using asuka.Core.Api.Queries;
using asuka.Core.Api.Responses;
using Refit;

namespace asuka.Core.Api;

public interface IGalleryApi
{
    [Get("/api/gallery/{code}")]
    Task<GalleryResponse> FetchSingle(string code);

    [Get("/api/gallery/{code}/related")]
    Task<GalleryListResponse> FetchRecommended(string code);

    [Get("/api/galleries/all?page=1")]
    Task<GalleryListResponse> FetchAll();

    [Get("/api/galleries/search")]
    Task<GallerySearchResponse> SearchGallery(SearchQuery queries);
}
