using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EntityDb.Common.Tests;

public class TestLogger<T> : ILogger<T>
{
    private readonly ILogger _logger;
    private readonly ITest _test;

    public TestLogger(ITest test, ILoggerFactory loggerFactory)
    {
        _test = test;
        _logger = new Logger<T>(loggerFactory);
    }

    IDisposable? ILogger.BeginScope<TState>(TState state)
    {
        return _logger.BeginScope(state);
    }

    bool ILogger.IsEnabled(LogLevel logLevel)
    {
        return _logger.IsEnabled(logLevel);
    }

    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        using (_logger.BeginScope($"Test: {_test.DisplayName}"))
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
