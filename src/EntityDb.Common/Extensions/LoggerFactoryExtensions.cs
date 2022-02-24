using EntityDb.Abstractions.Loggers;

namespace EntityDb.Common.Extensions;

internal static class LoggerFactoryExtensions
{
    public static ILogger CreateLogger<T>(this ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger(typeof(T));
    }
}
