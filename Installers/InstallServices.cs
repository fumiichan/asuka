using asuka.Commandline.Parsers;
using asuka.Configuration;
using asuka.Core.Compression;
using asuka.Core.Downloader;
using asuka.Core.Requests;
using asuka.Output.ProgressService;
using asuka.Output.Writer;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConfigurationManager = asuka.Configuration.ConfigurationManager;

namespace asuka.Installers;

public class InstallServices : IInstaller
{
    public void ConfigureService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AsukaApplication>();
        services.AddSingleton<IProgressService, ProgressService>();
        services.AddSingleton<IGalleryRequestService, GalleryRequestService>();
        services.AddSingleton<IConsoleWriter, ConsoleWriter>();
        services.AddScoped<IDownloadService, DownloadService>();
        services.AddScoped<IPackArchiveToCbz, PackArchiveToCbz>();

        // Command parsers
        services.AddScoped<IGetCommandService, GetCommandService>();
        services.AddScoped<IRecommendCommandService, RecommendCommandService>();
        services.AddScoped<ISearchCommandService, SearchCommandService>();
        services.AddScoped<IRandomCommandService, RandomCommandService>();
        services.AddScoped<IFileCommandService, FileCommandService>();
        services.AddScoped<IConfigurationManager, ConfigurationManager>();
        services.AddScoped<IConfigureCommand, ConfigureCommand>();
        services.AddValidatorsFromAssemblyContaining<Program>();
    }
}
