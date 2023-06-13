using asuka.Core.Chaptering;
using asuka.Core.Compression;
using asuka.Core.Configuration;
using asuka.Core.Downloader;
using asuka.Core.Output.Progress;
using Microsoft.Extensions.DependencyInjection;

namespace asuka.Application.Services;

public static class CoreExtensions
{
    public static void InstallCoreModules(this IServiceCollection services)
    {
        services.AddSingleton<IProgressService, asuka.Application.Output.ProgressService.ProgressService>();
        services.AddScoped<IDownloader, Downloader>();
        services.AddScoped<IPackArchiveToCbz, PackArchiveToCbz>();
        services.AddSingleton<IConfigurationManager, ConfigurationManager>();
        services.AddTransient<ISeriesFactory, SeriesFactory>();
    }
}
