using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using asuka.Core.Requests;
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
 
        var dlls = Directory.GetFiles(assemblyDir, "asuka.Providers.*.dll");
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
            ActivateInstances<IGalleryImageRequestService>(types, services);
            ActivateInstances<IGalleryRequestService>(types, services);
        }
    }

    private static void ActivateInstances<T>(IEnumerable<Type> types, IServiceCollection serviceCollection)
    {
        var services = types
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(T)));

        foreach (var service in services)
        {
            serviceCollection.AddSingleton(typeof(T), service);
        }
    }
}
