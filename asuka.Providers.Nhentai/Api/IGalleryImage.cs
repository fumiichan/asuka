using Refit;

namespace asuka.Providers.Nhentai.Api;

public interface IGalleryImage
{
    [Get("/galleries/{mediaId}/{filename}")]
    Task<HttpContent> GetImage(string mediaId, string filename);
}
