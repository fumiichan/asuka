using asuka.Core.Api.Responses;

namespace asuka.Core.Api;

public interface IGalleryApi
{
    Task<GalleryResponse> FetchSingle(string code);
    Task<GalleryListResponse> FetchRecommended(string code);
    Task<GalleryListResponse> FetchAll();
    Task<GallerySearchResponse> SearchGallery(string query, int pageNumber, string sort);
}
