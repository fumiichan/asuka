using asuka.Application.Output.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Services;

public static class LoggerExtensions
{
    public static void InstallLogger(this IServiceCollection services)
    {
        var asukaLogger = LoggerFactory.Create(logger =>
        {
            logger.ClearProviders();
            logger.AddCustomLogger();
        }).CreateLogger("asuka.Application");
        services.AddSingleton(asukaLogger);
    }
}
