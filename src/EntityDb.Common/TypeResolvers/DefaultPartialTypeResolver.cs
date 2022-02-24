using EntityDb.Common.Envelopes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EntityDb.Common.TypeResolvers;

internal class DefaultPartialTypeResolver : IPartialTypeResolver
{
    public bool TryResolveType(IReadOnlyDictionary<string, string> headers, [NotNullWhen(true)] out Type? resolvedType)
    {
        if (EnvelopeHelper.NotThisPlatform(headers) || !EnvelopeHelper.TryGetAssemblyFullName(headers, out var assemblyFullName) || !EnvelopeHelper.TryGetTypeFullName(headers, out var typeFullName))
        {
            resolvedType = null;
            return false;
        }

        resolvedType = Type.GetType
        (
            Assembly.CreateQualifiedName(assemblyFullName, typeFullName),
            Assembly.Load,
            (assembly, typeName, ignoreCase) => assembly!.GetType(typeName, true, ignoreCase),
            true,
            false
        )!;

        return true;
    }
}
