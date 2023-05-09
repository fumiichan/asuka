using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Installers.Logger;

public class InstallLogger : IInstaller
{
    public void ConfigureService(IServiceCollection services, IConfiguration configuration)
    {
        var asukaLogger = LoggerFactory.Create(logger =>
        {
            logger.ClearProviders();
            logger.AddConsole();
        }).CreateLogger("asuka.Application");
        
        services.AddSingleton<ILogger>(asukaLogger);
    }
}
