using EntityDb.Common.Envelopes;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EntityDb.Common.TypeResolvers;

internal sealed class DefaultPartialTypeResolver : IPartialTypeResolver
{
    private readonly IOptions<DefaultPartialTypeResolverOptions> _options;
    private readonly Dictionary<string, Type> _cache = new();

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

        var qualifiedTypeName = Assembly.CreateQualifiedName(assemblyFullName, typeFullName);

        if (_cache.TryGetValue(qualifiedTypeName, out resolvedType))
        {
            return true;
        }

        if (!TryGetType(qualifiedTypeName, out resolvedType))
        {
            return false;
        }

        _cache.Add(qualifiedTypeName, resolvedType);

        return true;
    }

    private bool TryGetType(string qualifiedTypeName, [NotNullWhen(true)] out Type? resolvedType)
    {
        foreach (var (oldNamespace, newNamespace) in _options.Value.UpdateNamespaces)
        {
            qualifiedTypeName = qualifiedTypeName.Replace(oldNamespace, newNamespace);
        }
        
        resolvedType = Type.GetType
        (
            qualifiedTypeName,
            AssemblyResolver,
            TypeResolver,
            _options.Value.ThrowOnError,
            _options.Value.IgnoreCase
        );

        return resolvedType is not null;
    }

    private Assembly AssemblyResolver(AssemblyName assemblyName)
    {
        if (_options.Value.IgnoreVersion)
        {
            assemblyName.Version = null;
        }

        return Assembly.Load(assemblyName);
    }

    private Type? TypeResolver(Assembly? assembly, string typeName, bool ignoreCase)
    {
        return assembly?.GetType
        (
            typeName,
            _options.Value.ThrowOnError,
            ignoreCase
        );
    }
}
