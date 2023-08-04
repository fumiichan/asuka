using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using asuka.Sdk.Providers.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace asuka.Application.Services;

public static class ExternalProviderExtensions
{
    public static void InstallExternalProviders(this IServiceCollection services)
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
        if (assemblyDir is null)
        {
            return;
        }

        var dllRoot = Path.Combine(assemblyDir, "providers");
        if (!Directory.Exists(dllRoot))
        {
            return;
        }
 
        var dlls = Directory.GetFiles(dllRoot, "asuka.Providers.*.dll", SearchOption.AllDirectories);
        foreach (var dll in dlls)
        {
            Type[] types;
            try
            {
                types = Assembly.LoadFrom(dll).GetExportedTypes();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load due to an exception");
                Console.WriteLine(e);
                continue;
            }
    
            // Load all classes inheriting IGalleryImageRequestService and
            // IGalleryRequestService
            InjectServicesFromMetadata(types, services);
        }
    }

    private static void InjectServicesFromMetadata(IEnumerable<Type> types, IServiceCollection serviceCollection)
    {
        var metadata = types
            .FirstOrDefault(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ProviderMetadata)));

        if (metadata is null)
        {
            return;
        }

        serviceCollection.AddSingleton(typeof(ProviderMetadata), metadata);
    }
}
