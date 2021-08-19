using System;

namespace EntityDb.Abstractions.Loggers
{
    public interface ILogger
    {
        void LogError(Exception exception, string message);
    }
}
