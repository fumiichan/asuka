using System.Reflection;
using asuka.Sdk.Providers.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace asuka.ProviderDependencyInjectionExtensions;

public static class ExternalProviderExtensions
{
    public static void InstallExternalProviders(this IServiceCollection serviceCollection)
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (assemblyDir is null)
        {
            throw new ArgumentNullException(assemblyDir, "Assembly location is null");
        }

        var dllRoot = Path.Combine(assemblyDir, "providers");
        
        // When the providers folder doesn't exist, this means that no providers are available
        // to be loaded. We can safely ignore this.
        if (!Directory.Exists(dllRoot))
        {
            return;
        }

        var dlls = Directory.GetFiles(dllRoot, "asuka.Providers.*.dll", SearchOption.AllDirectories);
        foreach (var dll in dlls)
        {
            try
            {
                var types = Assembly.LoadFrom(dll).GetExportedTypes();
                InjectServicesFromMetadata(types, serviceCollection);
            }
            catch
            {
                // We don't necessarily need to do anything here.
            }
        }
    }
    
    private static void InjectServicesFromMetadata(IEnumerable<Type> types, IServiceCollection serviceCollection)
    {
        var metadata = types
            .FirstOrDefault(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(ProviderMetadata)));

        if (metadata is null)
        {
            return;
        }

        serviceCollection.AddSingleton(typeof(ProviderMetadata), metadata);
    }
}
