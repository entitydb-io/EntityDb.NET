using EntityDb.Abstractions.Strategies;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Tests.Transactions;
using EntityDb.TestImplementations.Entities;
using System;

namespace EntityDb.MongoDb.Tests.Transactions
{
    public class TransactionTests : TransactionTestsBase
    {
        public TransactionTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
