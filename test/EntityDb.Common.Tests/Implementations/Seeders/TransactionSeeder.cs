using System.Collections.Immutable;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Agents;
using EntityDb.Common.Entities;
using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.Common.Transactions;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class TransactionSeeder
{
    public static ITransaction Create(params ITransactionCommand[] transactionCommands)
    {
        return new Transaction
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Commands = transactionCommands.ToImmutableArray()
        };
    }

    public static ITransaction Create<TEntity>(Id entityId, uint numCommands, ulong previousVersionNumberValue = 0)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        var transactionCommands = TransactionCommandSeeder.CreateFromCommands<TEntity>(entityId, numCommands, previousVersionNumberValue).ToArray();

        return Create(transactionCommands);
    }
}