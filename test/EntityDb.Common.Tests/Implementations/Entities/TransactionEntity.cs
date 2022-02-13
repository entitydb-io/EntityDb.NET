using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Tests.Implementations.Entities
{
    public record TransactionEntity
    (
        ulong VersionNumber = default,
        ILease[]? Leases = default,
        ITag[]? Tags = default
    )
        : IEntity<TransactionEntity>
    {
        public const string MongoCollectionName = "Test";
        public const string RedisKeyNamespace = "test";

        public static TransactionEntity Construct(Guid entityId)
        {
            return new();
        }

        public ulong GetVersionNumber()
        {
            return VersionNumber;
        }

        public IEnumerable<ILease> GetLeases()
        {
            if (Leases != null)
            {
                return Leases;
            }

            return Enumerable.Empty<ILease>();
        }

        public IEnumerable<ITag> GetTags()
        {
            if (Tags != null)
            {
                return Tags;
            }

            return Enumerable.Empty<ITag>();
        }
    }
}
