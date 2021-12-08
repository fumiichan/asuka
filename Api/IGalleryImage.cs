using System.Net.Http;
using System.Threading.Tasks;
using Refit;

namespace asuka.Api;

public interface IGalleryImage
{
    [Get("/galleries/{mediaId}/{filename}")]
    Task<HttpContent> GetImage(string mediaId, string filename);
}
