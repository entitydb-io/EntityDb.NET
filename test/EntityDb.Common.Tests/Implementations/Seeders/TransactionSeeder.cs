using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Tests.Implementations.Agents;
using EntityDb.Common.Transactions;
using EntityDb.Common.Transactions.Steps;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class TransactionStepSeeder
{
    public static IEnumerable<ITransactionStep> CreateFromCommands(Id entityId, uint numCommands)
    {
        for (var previousVersionNumber = new VersionNumber(0); previousVersionNumber.Value < numCommands; previousVersionNumber = previousVersionNumber.Next())
        {
            yield return new AppendCommandTransactionStep
            {
                EntityId = entityId,
                Entity = new object(),
                EntityVersionNumber = previousVersionNumber.Next(),
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

    public static ITransaction Create(Id entityId, uint numCommands)
    {
        var transactionSteps = TransactionStepSeeder.CreateFromCommands(entityId, numCommands).ToArray();

        return Create(transactionSteps);
    }
}