using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using asuka.Api;
using asuka.Api.Queries;
using asuka.Mappings;
using asuka.Models;

namespace asuka.Services;

public class GalleryRequestService : IGalleryRequestService
{
    private readonly IGalleryApi _api;

    public GalleryRequestService(IGalleryApi api)
    {
        _api = api;
    }

    public async Task<GalleryResult> FetchSingleAsync(string code)
    {
        var result = await _api.FetchSingle(code)
            .ConfigureAwait(false);
        return result.ToGalleryResult();
    }

    public async Task<IReadOnlyList<GalleryResult>> FetchRecommendedAsync(string code)
    {
        var result = await _api.FetchRecommended(code)
            .ConfigureAwait(false);
        return result.Result.Select(x => x.ToGalleryResult()).ToList();
    }

    public async Task<IReadOnlyList<GalleryResult>> SearchAsync(SearchQuery query)
    {
        var result = await _api.SearchGallery(query)
            .ConfigureAwait(false);
        return result.Result.Select(x => x.ToGalleryResult()).ToList();
    }

    public async Task<int> GetTotalGalleryCountAsync()
    {
        var result = await _api.FetchAll()
            .ConfigureAwait(false);
        var id = result.Result[0].Id;

        return id;
    }
}
