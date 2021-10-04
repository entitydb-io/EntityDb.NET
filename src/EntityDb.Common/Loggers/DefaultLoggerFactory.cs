using Microsoft.Extensions.Logging;
using System;
using ILogger = EntityDb.Abstractions.Loggers.ILogger;

namespace EntityDb.Common.Loggers
{
    internal record DefaultLoggerFactory(ILoggerFactory LoggerFactory) : Abstractions.Loggers.ILoggerFactory
    {
        public ILogger CreateLogger(Type type)
        {
            Microsoft.Extensions.Logging.ILogger? logger = LoggerFactory.CreateLogger(type);

            return new DefaultLogger(logger);
        }
    }
}
