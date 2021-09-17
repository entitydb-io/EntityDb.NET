using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Envelopes;
using System;
using System.Collections.Generic;
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

        public Type? ResolveType(IReadOnlyDictionary<string, string> headers)
        {
            if (EnvelopeHelper.NotThisPlatform(headers))
            {
                return null;
            }

            EnvelopeHelper.TryGetMemberInfoName(headers, out var memberInfoName);

            if (memberInfoName != null && _typeDictionary.TryGetValue(memberInfoName, out var resolvedType))
            {
                return resolvedType;
            }

            return null;
        }
    }
}
