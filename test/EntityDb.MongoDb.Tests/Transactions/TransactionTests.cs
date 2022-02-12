using EntityDb.Common.Tests.Transactions;
using System;

namespace EntityDb.MongoDb.Tests.Transactions
{
    public class TransactionTests : TransactionTestsBase<Startup>
    {
        public TransactionTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
        {
        }
    }
}
