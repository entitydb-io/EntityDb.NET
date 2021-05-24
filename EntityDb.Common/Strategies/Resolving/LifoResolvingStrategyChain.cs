using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Strategies.Resolving
{
    internal sealed class LifoResolvingStrategyChain : IResolvingStrategyChain
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IResolvingStrategy[] _resolvingStrategies;

        public LifoResolvingStrategyChain(IServiceProvider serviceProvider, IEnumerable<IResolvingStrategy> resolvingStrategies)
        {
            _serviceProvider = serviceProvider;
            _resolvingStrategies = resolvingStrategies.Reverse().ToArray();
        }

        public Type ResolveType(string? assemblyFullName, string? typeFullName, string? typeName)
        {
            var logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(LifoResolvingStrategyChain));

            foreach (var resolvingStrategy in _resolvingStrategies)
            {
                try
                {
                    var resolvedType = resolvingStrategy.ResolveType(assemblyFullName, typeFullName, typeName);

                    if (resolvedType != null)
                    {
                        return resolvedType;
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception.GetHashCode(), exception, "Resolving strategy threw an exception. Moving on to next resolving strategy.");
                }
            }

            throw new CannotResolveTypeException();
        }
    }
}
