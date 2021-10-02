using Microsoft.Extensions.Logging;
using System;

namespace EntityDb.Common.Loggers
{
    internal record DefaultLoggerFactory(ILoggerFactory LoggerFactory) : Abstractions.Loggers.ILoggerFactory
    {
        public Abstractions.Loggers.ILogger CreateLogger(Type type)
        {
            var logger = LoggerFactory.CreateLogger(type);

            return new DefaultLogger(logger);
        }
    }
}
