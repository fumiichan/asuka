using asuka.Provider.Sdk;
using asuka.Provider.Sdk.Utilities;
using Refit;

namespace asuka.Provider.Nhentai.Api.Client;

internal sealed class ImageRequestClient<T> where T : MetaInfo
{
    public IGalleryImage Client { get; private set; }

    public ImageRequestClient(string baseUrl)
    {
        var client = HttpClientFactory.CreateClientFromProvider<T>(baseUrl);
        Client = RestService.For<IGalleryImage>(client);
    }
}
