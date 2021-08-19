using EntityDb.Abstractions.Loggers;

namespace EntityDb.Common.Extensions
{
    internal static class ILoggerFactoryExtensions
    {
        public static ILogger CreateLogger<T>(this ILoggerFactory loggerFactory)
        {
            return loggerFactory.CreateLogger(typeof(T));
        }
    }
}
