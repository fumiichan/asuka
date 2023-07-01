using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace asuka.Application.Output.Logging;

public static class CustomLoggerInstaller
{
    public static ILoggingBuilder AddCustomLogger(this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, CustomLoggerProvider>());

        return builder;
    }
}
