using EntityDb.Common.Envelopes;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EntityDb.Common.TypeResolvers;

internal class DefaultPartialTypeResolver : IPartialTypeResolver
{
    public bool TryResolveType(EnvelopeHeaders envelopeHeaders, [NotNullWhen(true)] out Type? resolvedType)
    {
        if (EnvelopeHelper.NotThisPlatform(envelopeHeaders) ||
            !EnvelopeHelper.TryGetAssemblyFullName(envelopeHeaders, out var assemblyFullName) ||
            !EnvelopeHelper.TryGetTypeFullName(envelopeHeaders, out var typeFullName))
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
