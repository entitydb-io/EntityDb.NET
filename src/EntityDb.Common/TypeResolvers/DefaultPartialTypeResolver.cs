using EntityDb.Common.Envelopes;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EntityDb.Common.TypeResolvers;

internal class DefaultPartialTypeResolver : IPartialTypeResolver
{
    private readonly IOptions<DefaultPartialTypeResolverOptions> _options;

    public DefaultPartialTypeResolver(IOptions<DefaultPartialTypeResolverOptions> options)
    {
        _options = options;
    }

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
            AssemblyResolver,
            (assembly, typeName, ignoreCase) => assembly!.GetType(typeName, true, ignoreCase),
            true,
            false
        )!;

        return true;
    }

    private Assembly AssemblyResolver(AssemblyName assemblyName)
    {
        if (_options.Value.IgnoreVersion)
        {
            assemblyName.Version = null;
        }

        return Assembly.Load(assemblyName);
    }
}
