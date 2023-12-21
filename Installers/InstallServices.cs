using asuka.Commandline;
using asuka.Commandline.Parsers;
using asuka.Configuration;
using asuka.Core.Requests;
using asuka.Output.Progress;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace asuka.Installers;

public class InstallServices : IInstaller
{
    public void ConfigureService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IGalleryRequestService, GalleryRequestService>();
        services.AddSingleton<IAppConfigManager, AppConfigManager>();
        services.AddSingleton<IRequestConfigurator, RequestConfigurator>();
        services.AddSingleton<IProgressFactory, ProgressFactory>();

        // Command parsers
        services.AddKeyedScoped<ICommandLineParser, GetCommandService>("Cmd_Get");
        services.AddKeyedScoped<ICommandLineParser, RecommendCommandService>("Cmd_Recommend");
        services.AddKeyedScoped<ICommandLineParser, SearchCommandService>("Cmd_Search");
        services.AddKeyedScoped<ICommandLineParser, RandomCommandService>("Cmd_Random");
        services.AddKeyedScoped<ICommandLineParser, FileCommandService>("Cmd_File");
        services.AddKeyedScoped<ICommandLineParser, ConfigureCommand>("Cmd_Configure");
        services.AddKeyedScoped<ICommandLineParser, SeriesCreatorCommandService>("Cmd_Series");
        services.AddKeyedScoped<ICommandLineParser, CookieConfigureService>("Cmd_Cookie");
        services.AddValidatorsFromAssemblyContaining<Program>();
    }
}
