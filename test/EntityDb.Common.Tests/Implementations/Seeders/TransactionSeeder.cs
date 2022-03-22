using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Tests.Implementations.Agents;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Transactions;
using EntityDb.Common.Transactions.Steps;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class TransactionStepSeeder
{
    public static IEnumerable<ITransactionStep> CreateFromCommands<TEntity>(Id entityId, uint numCommands)
        where TEntity : IEntityWithVersionNumber<TEntity>
    {
        for (var previousVersionNumber = new VersionNumber(0); previousVersionNumber.Value < numCommands; previousVersionNumber = previousVersionNumber.Next())
        {
            var entityVersionNumber = previousVersionNumber.Next();
            
            yield return new AppendCommandTransactionStep
            {
                EntityId = entityId,
                Entity = TEntity.Construct(entityId, entityVersionNumber),
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
            AgentSignature = new NoAgentSignature(),
            Steps = transactionSteps.ToImmutableArray()
        };
    }

    public static ITransaction Create<TEntity>(Id entityId, uint numCommands)
        where TEntity : IEntityWithVersionNumber<TEntity>
    {
        var transactionSteps = TransactionStepSeeder.CreateFromCommands<TEntity>(entityId, numCommands).ToArray();

        return Create(transactionSteps);
    }
}