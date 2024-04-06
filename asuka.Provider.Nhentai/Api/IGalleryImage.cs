using Refit;

namespace asuka.Provider.Nhentai.Api;

internal interface IGalleryImage
{
    [Get("/galleries/{mediaId}/{filename}")]
    Task<HttpContent> GetImage(string mediaId, string filename, CancellationToken cancellationToken = default);
}
