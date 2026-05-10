using asuka.Provider.Nhentai.Api.Requests;
using asuka.Provider.Nhentai.Contracts;
using Refit;

namespace asuka.Provider.Nhentai.Api;

internal interface IGalleryApi
{
    [Get("/api/v2/cdn")]
    Task<GalleryConfigResponse> GetCdnAddresses(CancellationToken cancellationToken = default);
    
    [Get("/api/v2/galleries/{code}")]
    Task<GalleryResponse> FetchSingle(string code, CancellationToken cancellationToken = default);

    [Get("/api/v2/galleries/{code}/related")]
    Task<GallerySearchResponse> FetchRecommended(string code, CancellationToken cancellationToken = default);

    [Get("/api/v2/search")]
    Task<GallerySearchResponse> SearchGallery(GallerySearchQuery queries, CancellationToken cancellationToken = default);
}
