using Refit;

namespace asuka.Provider.Nhentai.Api;

internal interface IGalleryImage
{
    [Get("/galleries/{**route}")]
    Task<HttpContent> GetImage(string route, CancellationToken cancellationToken = default);
}
