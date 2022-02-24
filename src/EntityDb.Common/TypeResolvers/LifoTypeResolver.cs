using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.TypeResolvers;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.TypeResolvers;

internal sealed class LifoTypeResolver : ITypeResolver
{
    private readonly ILogger _logger;
    private readonly IPartialTypeResolver[] _partialTypeResolvers;

    public LifoTypeResolver(ILoggerFactory loggerFactory,
        IEnumerable<IPartialTypeResolver> partialTypeResolvers)
    {
        _logger = loggerFactory.CreateLogger<LifoTypeResolver>();
        _partialTypeResolvers = partialTypeResolvers.Reverse().ToArray();
    }

    public Type ResolveType(IReadOnlyDictionary<string, string> headers)
    {
        foreach (var partialTypeResolver in _partialTypeResolvers)
        {
            try
            {
                if (partialTypeResolver.TryResolveType(headers, out var resolvedType))
                {
                    return resolvedType;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Type resolver threw an exception. Moving on to next type resolver.");
            }
        }

        throw new CannotResolveTypeException();
    }
}
