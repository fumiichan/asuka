using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace asuka.Installers;

public static class InstallerExtension
{
    public static void InstallServicesInAssembly(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var installers = typeof(Program).Assembly.ExportedTypes
            .Where(x => typeof(IInstaller).IsAssignableFrom(x) && !x.IsInterface && !x.IsInterface)
            .Select(Activator.CreateInstance)
            .Cast<IInstaller>()
            .ToList();
        
        installers.ForEach(installer => installer.ConfigureService(serviceCollection, configuration));
    }
}
