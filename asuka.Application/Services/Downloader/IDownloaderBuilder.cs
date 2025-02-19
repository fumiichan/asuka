using asuka.Provider.Sdk;

namespace asuka.Application.Services.Downloader;

internal interface IDownloaderBuilder
{
    Downloader CreateDownloaderInstance(MetaInfo client, Series series);
}
