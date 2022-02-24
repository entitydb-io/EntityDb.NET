using EntityDb.Common.Envelopes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EntityDb.Common.TypeResolvers;

internal sealed class MemberInfoNamePartialTypeResolver : IPartialTypeResolver
{
    private readonly Dictionary<string, Type> _typeDictionary;

    public MemberInfoNamePartialTypeResolver(IEnumerable<Type> types)
    {
        _typeDictionary = types.ToDictionary(type => type.Name, type => type);
    }

    public bool TryResolveType(IReadOnlyDictionary<string, string> headers, [NotNullWhen(true)] out Type? resolvedType)
    {
        if (EnvelopeHelper.NotThisPlatform(headers) || !EnvelopeHelper.TryGetMemberInfoName(headers, out var memberInfoName) || !_typeDictionary.TryGetValue(memberInfoName, out var type))
        {
            resolvedType = null;
            return false;
        }

        resolvedType = type;
        return true;
    }
}
