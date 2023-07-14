using Microsoft.Extensions.Logging;

namespace asuka.Core.Events;

public class DummyLogging : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        // do nothing.
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return false;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;
}
