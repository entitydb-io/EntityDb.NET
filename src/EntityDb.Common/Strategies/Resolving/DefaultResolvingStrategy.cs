using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Envelopes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EntityDb.Common.Strategies.Resolving
{
    internal class DefaultResolvingStrategy : IResolvingStrategy
    {
        public Type? ResolveType(IReadOnlyDictionary<string, string> headers)
        {
            if (EnvelopeHelper.NotThisPlatform(headers))
            {
                return null;
            }

            EnvelopeHelper.TryGetAssemblyFullName(headers, out string? assemblyFullName);
            EnvelopeHelper.TryGetTypeFullName(headers, out string? typeFullName);

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
