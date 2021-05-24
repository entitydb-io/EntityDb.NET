using EntityDb.Common.Queries;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Sessions;
using EntityDb.MongoDb.Transactions;
using EntityDb.TestImplementations.Entities;
using Mongo2Go;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.MongoDb.Tests.Sessions
{
    public class MongoDbSessionTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MongoDbRunner _mongoDbRunner;

        public MongoDbSessionTests(IServiceProvider serviceProvider, MongoDbRunner mongoDbRunner)
        {
            _serviceProvider = serviceProvider;
            _mongoDbRunner = mongoDbRunner;
        }

        [Fact]
        public async Task GivenInvalidMongoDbSession_WhenTryingToExecuteQuery_ThenReturnEmpty()
        {
            // ARRANGE

            var mongoDbSession = new MongoDbSession(_serviceProvider, default!, default!);

            var mongoDbRepository = new MongoDbTransactionRepository<TransactionEntity>(mongoDbSession);

            // ACT

            var facts = await mongoDbRepository.GetFacts(new GetEntityQuery(Guid.NewGuid(), 0));

            // ASSERT

            Assert.Empty(facts);
        }

        [Fact]
        public async Task GivenValidTestModeMongoDbSession_WhenWritingInReadOnly_ThenReturnFalse()
        {
            // ARRANGE

            var mongoDbTransactionRepositoryFactory = new TestModeMongoDbTransactionRepositoryFactory<TransactionEntity>(_serviceProvider, _mongoDbRunner.ConnectionString, "Fake");

            var mongoDbSession = await mongoDbTransactionRepositoryFactory.CreateSession(new TransactionSessionOptions
            {
                ReadOnly = true,
                SecondaryPreferred = true,
            });

            // ACT

            var executed = await mongoDbSession.ExecuteCommand((logger, clientSessionHandle, mongoDatabase) => Task.CompletedTask);

            // ASSERT

            Assert.False(executed);
        }
    }
}
