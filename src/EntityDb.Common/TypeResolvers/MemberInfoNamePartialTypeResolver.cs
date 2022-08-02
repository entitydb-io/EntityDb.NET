using EntityDb.Common.Envelopes;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Common.TypeResolvers;

internal sealed class MemberInfoNamePartialTypeResolver : IPartialTypeResolver
{
    private readonly Dictionary<string, Type> _typeDictionary;

    public MemberInfoNamePartialTypeResolver(IEnumerable<Type> types)
    {
        _typeDictionary = types.ToDictionary(type => type.Name, type => type);
    }

    public bool TryResolveType(EnvelopeHeaders envelopeHeaders, [NotNullWhen(true)] out Type? resolvedType)
    {
        if (EnvelopeHelper.NotThisPlatform(envelopeHeaders) ||
            !EnvelopeHelper.TryGetMemberInfoName(envelopeHeaders, out var memberInfoName) ||
            !_typeDictionary.TryGetValue(memberInfoName, out var type))
        {
            resolvedType = null;
            return false;
        }

        resolvedType = type;
        return true;
    }
}
