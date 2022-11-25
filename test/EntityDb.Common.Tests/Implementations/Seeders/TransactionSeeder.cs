using System.Collections.Immutable;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Agents;
using EntityDb.Common.Entities;
using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.Common.Transactions;
using EntityDb.Common.Transactions.Steps;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class TransactionStepSeeder
{
    public static IEnumerable<ITransactionStep> CreateFromCommands<TEntity>(Id entityId, uint numCommands)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        for (var previousVersionNumber = new VersionNumber(0);
             previousVersionNumber.Value < numCommands;
             previousVersionNumber = previousVersionNumber.Next())
        {
            var entityVersionNumber = previousVersionNumber.Next();

            yield return new AppendCommandTransactionStep
            {
                EntityId = entityId,
                Entity = TEntity.Construct(entityId).WithVersionNumber(entityVersionNumber),
                EntityVersionNumber = entityVersionNumber,
                PreviousEntityVersionNumber = previousVersionNumber,
                Command = CommandSeeder.Create()
            };
        }
    }
}

public static class TransactionSeeder
{
    public static ITransaction Create(params ITransactionStep[] transactionSteps)
    {
        return new Transaction
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Steps = transactionSteps.ToImmutableArray()
        };
    }

    public static ITransaction Create<TEntity>(Id entityId, uint numCommands)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        var transactionSteps = TransactionStepSeeder.CreateFromCommands<TEntity>(entityId, numCommands).ToArray();

        return Create(transactionSteps);
    }
}