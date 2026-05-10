using asuka.Provider.Nhentai.Api.Client;
using Refit;

namespace asuka.Provider.Nhentai.Api;

internal interface IGalleryImage
{
    [Get("/{**route}")]
    Task<HttpContent> GetImage(string route, CancellationToken cancellationToken = default);
}

internal static class GalleryImageUtility
{
    public static async Task<List<ImageRequestClient<Provider>>> GetImageProviders(IGalleryApi client)
    {
        var result = await client.GetCdnAddresses();
        List<ImageRequestClient<Provider>> clients = new() { };

        foreach (var host in result.ImageServers)
        {
            clients.Add(new ImageRequestClient<Provider>(host));
        }
        
        return clients;
    }
}

internal sealed class GalleryImageProvider
{
    private readonly List<ImageRequestClient<Provider>> _clients = [];

    public async Task<List<ImageRequestClient<Provider>>> GetImageProviders(IGalleryApi client)
    {
        // Return the clients if available.
        if (_clients.Count > 0) return _clients;
        
        // Fetch the clients
        var result = await client.GetCdnAddresses();

        foreach (var host in result.ImageServers)
        {
            _clients.Add(new ImageRequestClient<Provider>(host));
        }

        return _clients;
    }
}
