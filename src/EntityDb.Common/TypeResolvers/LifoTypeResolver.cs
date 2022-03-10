using EntityDb.Common.Envelopes;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.TypeResolvers;

internal sealed class LifoTypeResolver : ITypeResolver
{
    private readonly ILogger<LifoTypeResolver> _logger;
    private readonly IPartialTypeResolver[] _partialTypeResolvers;

    public LifoTypeResolver(ILogger<LifoTypeResolver> logger,
        IEnumerable<IPartialTypeResolver> partialTypeResolvers)
    {
        _logger = logger;
        _partialTypeResolvers = partialTypeResolvers.Reverse().ToArray();
    }

    public Type ResolveType(EnvelopeHeaders headers)
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
                    "Type resolver threw an exception. Moving on to next partial type resolver.");
            }
        }

        throw new CannotResolveTypeException();
    }
}
