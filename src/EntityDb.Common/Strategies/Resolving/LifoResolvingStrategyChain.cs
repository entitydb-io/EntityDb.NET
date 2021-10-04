using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Strategies.Resolving
{
    internal sealed class LifoResolvingStrategyChain : IResolvingStrategyChain
    {
        private readonly ILogger _logger;
        private readonly IResolvingStrategy[] _resolvingStrategies;

        public LifoResolvingStrategyChain(ILoggerFactory loggerFactory,
            IEnumerable<IResolvingStrategy> resolvingStrategies)
        {
            _logger = loggerFactory.CreateLogger<LifoResolvingStrategyChain>();
            _resolvingStrategies = resolvingStrategies.Reverse().ToArray();
        }

        public Type ResolveType(IReadOnlyDictionary<string, string> headers)
        {
            foreach (var resolvingStrategy in _resolvingStrategies)
            {
                try
                {
                    Type? resolvedType = resolvingStrategy.ResolveType(headers);

                    if (resolvedType != null)
                    {
                        return resolvedType;
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception,
                        "Resolving strategy threw an exception. Moving on to next resolving strategy.");
                }
            }

            throw new CannotResolveTypeException();
        }
    }
}
