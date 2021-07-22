using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace EntityDb.TestImplementations.Entities
{
    public record TransactionEntity
    (
        ulong VersionNumber = default,
        string? Role = default,
        ILease[]? Leases = default
    ) :
        IAuthorizedEntity<TransactionEntity>,
        IVersionedEntity<TransactionEntity>,
        ILeasedEntity
    {
        public const string MongoCollectionName = "Test";
        public const string RedisKeyNamespace = "test";

        public bool IsAuthorized(ICommand<TransactionEntity> command, ClaimsPrincipal claimsPrincipal)
        {
            if (Role != null)
            {
                return claimsPrincipal.IsInRole(Role);
            }

            return true;
        }

        public TransactionEntity WithVersionNumber(ulong versionNumber)
        {
            return this with { VersionNumber = versionNumber };
        }

        public IEnumerable<ILease> GetLeases()
        {
            if (Leases != null)
            {
                return Leases;
            }

            return Enumerable.Empty<ILease>();
        }
    }
}
