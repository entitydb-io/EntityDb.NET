using EntityDb.Abstractions.Strategies;
using System;
using System.Reflection;

namespace EntityDb.Common.Strategies.Resolving
{
    internal class DefaultResolvingStrategy : IResolvingStrategy
    {
        public Type? ResolveType(string? assemblyFullName, string? typeFullName, string? typeName)
        {
            if (assemblyFullName == null || typeFullName == null)
            {
                return null;
            }

            return Type.GetType
            (
                Assembly.CreateQualifiedName(assemblyFullName, typeFullName),
                Assembly.Load,
                (assembly, typeName, ignoreCase) => assembly!.GetType(typeName, true, ignoreCase),
                false,
                false
            );
        }
    }
}
