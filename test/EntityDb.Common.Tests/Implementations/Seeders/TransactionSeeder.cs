using System.Collections.Immutable;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Tests.Implementations.Agents;
using EntityDb.Common.Transactions;

namespace EntityDb.Common.Tests.Implementations.Seeders;

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
}