using System;
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace asuka.Application.Output.Logging;

[UnsupportedOSPlatform("browser")]
public sealed class CustomLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, CustomLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public void Dispose()
    {
        _loggers.Clear();
    }

    public ILogger CreateLogger(string categoryName)
        => _loggers.GetOrAdd(categoryName, _ => new CustomLogger());
}
