using EntityDb.Common.Entities;
using System;
using System.Linq;
using EntityDb.Abstractions.Reducers;

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

    public TransactionEntity Reduce(object[] commands)
    {
        var newEntity = this;

        foreach (var command in commands)
        {
            if (command is IReducer<TransactionEntity> reducer)
            {
                newEntity = reducer.Reduce(newEntity);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        return newEntity;
    }
}