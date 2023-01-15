using asuka.Commandline.Parsers;
using asuka.Configuration;
using asuka.Core.Api;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Output.Writer;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConfigurationManager = asuka.Configuration.ConfigurationManager;

namespace asuka.Installers;

public class ServiceCollection
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AsukaApplication>();
        services.AddSingleton<IGalleryRequestService, GalleryRequestService>();
        services.AddSingleton<IConsoleWriter, ConsoleWriter>();
        services.AddSingleton<IGetCommandService, GetCommandService>();
        services.AddSingleton<IDownloadService, DownloadService>();
        services.AddSingleton<IRecommendCommandService, RecommendCommandService>();
        services.AddSingleton<ISearchCommandService, SearchCommandService>();
        services.AddSingleton<IRandomCommandService, RandomCommandService>();
        services.AddSingleton<IFileCommandService, FileCommandService>();
        services.AddSingleton<IPackArchiveToCbz, PackArchiveToCbz>();
        services.AddSingleton<IConfigurationManager, ConfigurationManager>();
        services.AddSingleton<IConfigureCommand, ConfigureCommand>();
        services.AddValidatorsFromAssemblyContaining<Program>();
    }
}
