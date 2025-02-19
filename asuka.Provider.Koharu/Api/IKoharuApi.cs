using asuka.Provider.Koharu.Contracts.Queries;
using asuka.Provider.Koharu.Contracts.Responses;
using Refit;

namespace asuka.Provider.Koharu.Api;

internal interface IKoharuApi
{
    [Get("/books/detail/{id}/{publicKey}")]
    Task<GalleryInfoResponse> FetchSingle(int id, string publicKey, CancellationToken cancellationToken = default);

    [Get("/books/random")]
    Task<GalleryRandomResponse> FetchRandom(CancellationToken cancellationToken = default);
    
    [Get("/books/data/{id}/{publicKey}/{anotherId}/{anotherPublicKey}")]
    Task<GalleryContentsResponse> FetchContents(
        int id,
        string publicKey,
        int anotherId,
        string anotherPublicKey,
        ImageListQuery query,
        CancellationToken cancellationToken = default);
    
    Task<GallerySearchResult> Search(SearchParams @params, CancellationToken cancellationToken = default);
}
