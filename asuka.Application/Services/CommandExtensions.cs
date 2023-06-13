using asuka.Application.Commandline;
using asuka.Application.Commandline.Parsers;
using Microsoft.Extensions.DependencyInjection;

namespace asuka.Application.Services;

public static class CommandExtensions
{
    public static void InstallCommandParsers(this IServiceCollection services)
    {
        services.AddScoped<ICommandLineParser, GetCommandService>();
        services.AddScoped<ICommandLineParser, RecommendCommandService>();
        services.AddScoped<ICommandLineParser, SearchCommandService>();
        services.AddScoped<ICommandLineParser, RandomCommandService>();
        services.AddScoped<ICommandLineParser, FileCommandService>();
        services.AddScoped<ICommandLineParser, ConfigureCommand>();
        services.AddScoped<ICommandLineParser, SeriesCreatorCommandService>();
        services.AddScoped<ICommandLineParserFactory, CommandLineParserFactory>();
    }
}
