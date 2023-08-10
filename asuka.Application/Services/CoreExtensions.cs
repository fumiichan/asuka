using asuka.Application.Output.Progress;
using Microsoft.Extensions.DependencyInjection;

namespace asuka.Application.Services;

public static class CoreExtensions
{
    public static void InstallCoreModules(this IServiceCollection services)
    {
        services.AddSingleton<IProgressProviderFactory, ProgressProviderFactory>();
        services.AddSingleton<ProviderResolverService>();
    }
}
