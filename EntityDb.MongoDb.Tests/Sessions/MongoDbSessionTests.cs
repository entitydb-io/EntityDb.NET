using EntityDb.Abstractions.Loggers;
using EntityDb.Common.Extensions;
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
        private readonly ILogger _logger;

        public MongoDbSessionTests(IServiceProvider serviceProvider, MongoDbRunner mongoDbRunner, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _mongoDbRunner = mongoDbRunner;
            _logger = loggerFactory.CreateLogger<MongoDbSessionTests>();
        }

        [Fact]
        public async Task GivenInvalidMongoDbSession_WhenTryingToExecuteQuery_ThenReturnEmpty()
        {
            // ARRANGE

            var mongoDbSession = new MongoDbSession(default!, default!, _logger, default!);

            var mongoDbRepository = new MongoDbTransactionRepository<TransactionEntity>(mongoDbSession);

            // ACT

            var facts = await mongoDbRepository.GetFacts(new GetEntityQuery(Guid.NewGuid(), 0));

            // ASSERT

            Assert.Empty(facts);
        }
    }
}
