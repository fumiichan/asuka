using asuka.Commandline;
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
        services.AddScoped<IConfigurationManager, ConfigurationManager>();

        // Command parsers
        services.AddScoped<ICommandLineParser, GetCommandService>();
        services.AddScoped<ICommandLineParser, RecommendCommandService>();
        services.AddScoped<ICommandLineParser, SearchCommandService>();
        services.AddScoped<ICommandLineParser, RandomCommandService>();
        services.AddScoped<ICommandLineParser, FileCommandService>();
        services.AddScoped<ICommandLineParser, ConfigureCommand>();
        services.AddScoped<ICommandLineParserFactory, CommandLineParserFactory>();
        services.AddValidatorsFromAssemblyContaining<Program>();
    }
}
