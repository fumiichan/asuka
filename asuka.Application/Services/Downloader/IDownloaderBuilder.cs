using asuka.ProviderSdk;

namespace asuka.Application.Services.Downloader;

internal interface IDownloaderBuilder
{
    DownloadBuilder.Downloader CreateDownloaderInstance();
}
