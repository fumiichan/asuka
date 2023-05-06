using System.Net.Http;
using System.Threading.Tasks;
using Refit;

namespace asuka.Application.Api;

public interface IGalleryImage : asuka.Core.Api.IGalleryImage
{
    [Get("/galleries/{mediaId}/{filename}")]
    new Task<HttpContent> GetImage(string mediaId, string filename);
}
