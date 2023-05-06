namespace asuka.Core.Api;

public interface IGalleryImage
{
    Task<HttpContent> GetImage(string mediaId, string filename);
}
