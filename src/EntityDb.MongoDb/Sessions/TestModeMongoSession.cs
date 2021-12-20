using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Transactions;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal sealed record TestModeMongoSession : MongoSession
    {
        private readonly TestModeTransactionManager _testModeTransactionManager;

        public TestModeMongoSession(IClientSessionHandle clientSessionHandle,
            TestModeTransactionManager testModeTransactionManager) : base(clientSessionHandle)
        {
            _testModeTransactionManager = testModeTransactionManager;

            if (_testModeTransactionManager.NoHolds())
            {
                base.StartTransaction();
            }

            _testModeTransactionManager.Hold(nameof(TestModeMongoSession));
        }

        public override void StartTransaction()
        {
            if (!ClientSessionHandle.IsInTransaction)
            {
                throw new TestModeException();
            }
        }

        public override Task CommitTransaction()
        {
            if (!ClientSessionHandle.IsInTransaction)
            {
                throw new TestModeException();
            }

            return Task.CompletedTask;
        }

        public override Task AbortTransaction()
        {
            if (!ClientSessionHandle.IsInTransaction)
            {
                throw new TestModeException();
            }

            return Task.CompletedTask;
        }

        public override async ValueTask DisposeAsync()
        {
            _testModeTransactionManager.Release(nameof(TestModeMongoSession));

            if (_testModeTransactionManager.NoHolds())
            {
                await base.AbortTransaction();
            }

            await base.DisposeAsync();
        }
    }
}
