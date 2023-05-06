using asuka.Api;
using asuka.Api.Queries;
using asuka.Core.Mappings;
using asuka.Core.Models;

namespace asuka.Core.Requests;

public class GalleryRequestService : IGalleryRequestService
{
    private readonly IGalleryApi _api;

    public GalleryRequestService(IGalleryApi api)
    {
        _api = api;
    }

    public async Task<GalleryResult> FetchSingle(string code)
    {
        var result = await _api.FetchSingle(code)
            .ConfigureAwait(false);
        return result.ToGalleryResult();
    }

    public async Task<IReadOnlyList<GalleryResult>> FetchRecommended(string code)
    {
        var result = await _api.FetchRecommended(code)
            .ConfigureAwait(false);
        return result.Result.Select(x => x.ToGalleryResult()).ToList();
    }

    public async Task<IReadOnlyList<GalleryResult>> Search(SearchQuery query)
    {
        var result = await _api.SearchGallery(query)
            .ConfigureAwait(false);
        return result.Result.Select(x => x.ToGalleryResult()).ToList();
    }

    public async Task<int> GetTotalGalleryCount()
    {
        var result = await _api.FetchAll()
            .ConfigureAwait(false);
        var id = result.Result[0].Id;

        return id;
    }
}
