using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Tests.Implementations.AgentSignature;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Transactions;
using System;

namespace EntityDb.Common.Tests.Implementations.Seeders
{
    public static class TransactionSeeder
    {
        public static ITransaction<TransactionEntity> Create
        (
            int generateCount,
            int repeatCount,
            Guid? transactionId = null,
            ulong? previousEntityVersionNumber = null,
            bool wellBehavedNextEntityVersionNumber = true,
            Guid? entityId = null,
            bool insertLease = false,
            bool deleteLease = false,
            bool insertTag = false,
            bool deleteTag = false
        )
        {
            return new Transaction<TransactionEntity>
            {
                Id = transactionId ?? Guid.NewGuid(),
                TimeStamp = DateTime.UtcNow,
                AgentSignature = new NoAgentSignature(),
                Steps = TransactionStepSeeder.Create(generateCount, repeatCount, previousEntityVersionNumber,
                    wellBehavedNextEntityVersionNumber, entityId, insertLease, deleteLease, insertTag, deleteTag)
            };
        }
    }
}
