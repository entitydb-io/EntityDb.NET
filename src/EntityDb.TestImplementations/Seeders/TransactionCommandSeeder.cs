using Bogus;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Transactions;
using EntityDb.TestImplementations.Entities;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace EntityDb.TestImplementations.Seeders
{
    public static class TransactionCommandSeeder
    {
        public static ImmutableArray<ITransactionCommand<TransactionEntity>> Create
        (
            int generateCount,
            int repeatCount,
            ulong? previousEntityVersionNumber = null,
            bool wellBehavedNextEntityVersionNumber = true,
            Guid? entityId = null,
            bool insertLease = false,
            bool deleteLease = false,
            bool insertTag = false,
            bool deleteTag = false
        )
        {
            var faker = new Faker<TransactionCommand<TransactionEntity>>()
                .RuleFor
                (
                    transactionCommand => transactionCommand.PreviousEntityVersionNumber,
                    () => previousEntityVersionNumber ?? default
                )
                .RuleFor
                (
                    transactionCommand => transactionCommand.PreviousEntitySnapshot,
                    (_, transactionCommand) => new TransactionEntity().Reduce(Enumerable.Repeat(CommandSeeder.Create(), (int)transactionCommand.PreviousEntityVersionNumber))
                )
                .RuleFor
                (
                    transactionCommand => transactionCommand.Command,
                    () => CommandSeeder.Create()
                )
                .RuleFor
                (
                    transactionCommand => transactionCommand.NextEntityVersionNumber,
                    (_, transactionCommand) => wellBehavedNextEntityVersionNumber
                        ? transactionCommand.PreviousEntityVersionNumber + 1
                        : transactionCommand.PreviousEntityVersionNumber
                )
                .RuleFor
                (
                    transactionCommand => transactionCommand.NextEntitySnapshot,
                    (_, transactionCommand) =>transactionCommand.PreviousEntitySnapshot.Reduce(transactionCommand.Command)
                )
                .RuleFor
                (
                    transactionCommand => transactionCommand.EntityId,
                    () => entityId ?? Guid.NewGuid()
                )
                .RuleFor
                (
                    transactionCommand => transactionCommand.Leases,
                    () => TransactionMetaDataSeeder.ForLease(insertLease, deleteLease)
                )
                .RuleFor
                (
                    transactionCommand => transactionCommand.Tags,
                    () => TransactionMetaDataSeeder.ForTag(insertTag, deleteTag)
                );

            return Enumerable
                .Repeat(faker.Generate(generateCount), repeatCount)
                .SelectMany(x => x)
                .ToImmutableArray<ITransactionCommand<TransactionEntity>>();
        }
    }
}
