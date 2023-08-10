using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace asuka.Application.Services.Configuration;

public static class ConfigurationAsService
{
    public static void InstallConfigurationAsService(this IServiceCollection serviceCollection)
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var config = configuration
                .GetRequiredSection("ApplicationConfig")
                .Get<AsukaConfiguration>();

            serviceCollection.AddSingleton<AsukaConfiguration>(config);
        }
        catch
        {
            serviceCollection.AddSingleton<AsukaConfiguration>(new AsukaConfiguration());
        }
    }
}
