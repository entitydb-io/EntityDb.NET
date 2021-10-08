using Bogus;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using System.Collections.Immutable;

namespace EntityDb.TestImplementations.Seeders
{
    public static class TransactionMetaDataSeeder
    {
        public static ITransactionMetaData<ILease> ForLease(bool insert, bool delete)
        {
            var faker = new Faker<TransactionMetaData<ILease>>()
                .RuleFor
                (
                    transactionMetaData => transactionMetaData.Insert,
                    () => insert ? LeaseSeeder.Create() : ImmutableArray<ILease>.Empty
                )
                .RuleFor
                (
                    transactionMetaData => transactionMetaData.Delete,
                    () => delete ? LeaseSeeder.Create() : ImmutableArray<ILease>.Empty
                );

            return faker.Generate();
        }
        
        public static ITransactionMetaData<ITag> ForTag(bool insert, bool delete)
        {
            var faker = new Faker<TransactionMetaData<ITag>>()
                .RuleFor
                (
                    transactionMetaData => transactionMetaData.Insert,
                    () => insert ? TagSeeder.Create() : ImmutableArray<ITag>.Empty
                )
                .RuleFor
                (
                    transactionMetaData => transactionMetaData.Delete,
                    () => delete ? TagSeeder.Create() : ImmutableArray<ITag>.Empty
                );

            return faker.Generate();
        }
    }
}
