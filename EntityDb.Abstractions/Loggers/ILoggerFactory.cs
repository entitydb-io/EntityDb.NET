using System;

namespace EntityDb.Abstractions.Loggers
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(Type type);
    }
}
