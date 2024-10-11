using asuka.Provider.Nhentai.Api.Requests;
using asuka.Provider.Nhentai.Contracts;
using Refit;

namespace asuka.Provider.Nhentai.Api;

internal interface IGalleryApi
{
    [Get("/api/gallery/{code}")]
    Task<GalleryResponse> FetchSingle(string code, CancellationToken cancellationToken = default);

    [Get("/api/gallery/{code}/related")]
    Task<GalleryListResponse> FetchRecommended(string code, CancellationToken cancellationToken = default);

    [Get("/api/galleries/search")]
    Task<GallerySearchResponse> SearchGallery(GallerySearchQuery queries, CancellationToken cancellationToken = default);
}
