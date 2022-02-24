using System;
using System.Collections.Immutable;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Common.Tests.Implementations.Agents;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Transactions;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class TransactionSeeder
{
    public static ITransaction<TransactionEntity> Create(params ITransactionStep<TransactionEntity>[] transactionSteps)
    {
        return new Transaction<TransactionEntity>
        {
            Id = Guid.NewGuid(),
            TimeStamp = DateTime.UtcNow,
            AgentSignature = new NoAgentSignature(),
            Steps = transactionSteps.ToImmutableArray()
        };
    }
}