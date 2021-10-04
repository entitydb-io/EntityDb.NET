using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Common.Entities;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.TestImplementations.Entities
{
    public record TransactionEntity
    (
        ulong VersionNumber = default,
        string? Role = default,
        ILease[]? Leases = default,
        ITag[]? Tags = default
    ) :
        IAuthorizedEntity<TransactionEntity>,
        IVersionedEntity<TransactionEntity>,
        ILeasedEntity,
        ITaggedEntity
    {
        public const string MongoCollectionName = "Test";
        public const string RedisKeyNamespace = "test";

        public bool IsAuthorized(ICommand<TransactionEntity> command, IAgent agent)
        {
            if (Role != null)
            {
                return agent.HasRole(Role);
            }

            return true;
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

        public TransactionEntity WithVersionNumber(ulong versionNumber)
        {
            return this with { VersionNumber = versionNumber };
        }
    }
}
