using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Envelopes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EntityDb.Common.Strategies.Resolving
{
    internal sealed class MemberInfoNameResolvingStrategy : IResolvingStrategy
    {
        private readonly Dictionary<string, Type> _typeDictionary;

        public MemberInfoNameResolvingStrategy(Type[] types)
        {
            _typeDictionary = types.ToDictionary(type => type.Name, Type => Type);
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
}
