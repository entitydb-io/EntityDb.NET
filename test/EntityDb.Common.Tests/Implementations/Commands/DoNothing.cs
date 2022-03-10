using EntityDb.Abstractions.Reducers;
using EntityDb.Common.Tests.Implementations.Entities;

namespace EntityDb.Common.Tests.Implementations.Commands;

public record DoNothing : IReducer<TransactionEntity>
{
    public TransactionEntity Reduce(TransactionEntity entity)
    {
        return entity with { VersionNumber = entity.VersionNumber.Next() };
    }
}