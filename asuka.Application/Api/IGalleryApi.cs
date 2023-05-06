using System.Threading.Tasks;
using asuka.Core.Api.Responses;
using Refit;

namespace asuka.Application.Api;

public interface IGalleryApi : asuka.Core.Api.IGalleryApi
{
    [Get("/api/gallery/{code}")]
    new Task<GalleryResponse> FetchSingle(string code);

    [Get("/api/gallery/{code}/related")]
    new Task<GalleryListResponse> FetchRecommended(string code);

    [Get("/api/galleries/all?page=1")]
    new Task<GalleryListResponse> FetchAll();

    [Get("/api/galleries/search")]
    new Task<GallerySearchResponse> SearchGallery([AliasAs("query")] string query, [AliasAs("page")] int pageNumber, [AliasAs("sort")] string sort);
}
