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
        var assemblyDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        var dlls = Directory.GetFiles(assemblyDir, "asuka.Providers.*.dll");

        foreach (var dll in dlls)
        {
            Type[] types;
            try
            {
                types = Assembly.LoadFrom(dll).GetTypes();
            }
            catch
            {
                continue;
            }
    
            // Load all classes inheriting IGalleryImageRequestService and
            // IGalleryRequestService
            foreach (var installableImageRequestService in ActivateInstances<IGalleryImageRequestService>(types))
            {
                services.AddSingleton(installableImageRequestService);
            }
    
            foreach (var installableRequestService in ActivateInstances<IGalleryRequestService>(types))
            {
                services.AddSingleton(installableRequestService);
            }
        }
    }

    private static IEnumerable<T> ActivateInstances<T>(IEnumerable<Type> types)
    {
        return types
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(T)))
            .Select(Activator.CreateInstance)
            .Cast<T>();
    }
}
