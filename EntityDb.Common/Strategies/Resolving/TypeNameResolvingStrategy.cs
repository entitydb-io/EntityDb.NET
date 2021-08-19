using EntityDb.Abstractions.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Strategies.Resolving
{
    internal sealed class TypeNameResolvingStrategy : IResolvingStrategy
    {
        private readonly Dictionary<string, Type> _typeDictionary;

        public TypeNameResolvingStrategy(Type[] types)
        {
            _typeDictionary = types.ToDictionary(type => type.Name, Type => Type);
        }

        public Type? ResolveType(string? assemblyFullName, string? typeFullName, string? typeName)
        {
            if (typeName != null && _typeDictionary.TryGetValue(typeName, out var resolvedType))
            {
                return resolvedType;
            }

            return null;
        }
    }
}
