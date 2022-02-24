using EntityDb.Common.Entities;
using System;

namespace EntityDb.Common.Tests.Implementations.Entities;

public record TransactionEntity
    (
        ulong VersionNumber = default
    )
    : IEntity<TransactionEntity>
{
    public const string MongoCollectionName = "Test";
    public const string RedisKeyNamespace = "test";

    public static TransactionEntity Construct(Guid entityId)
    {
        return new TransactionEntity();
    }

    public ulong GetVersionNumber()
    {
        return VersionNumber;
    }
}