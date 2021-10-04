using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Loggers;
using EntityDb.Common.Extensions;
using EntityDb.Common.Queries;
using EntityDb.MongoDb.Sessions;
using EntityDb.MongoDb.Transactions;
using EntityDb.TestImplementations.Entities;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.MongoDb.Tests.Sessions
{
    public class MongoDbSessionTests
    {
        private readonly ILogger _logger;

        public MongoDbSessionTests(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MongoDbSessionTests>();
        }

        [Fact]
        public async Task GivenInvalidMongoDbSession_WhenTryingToExecuteQuery_ThenReturnEmpty()
        {
            // ARRANGE

            MongoDbSession? mongoDbSession = new MongoDbSession(default!, default!, _logger, default!);

            MongoDbTransactionRepository<TransactionEntity>? mongoDbRepository =
                new MongoDbTransactionRepository<TransactionEntity>(mongoDbSession);

            // ACT

            IFact<TransactionEntity>[]? facts = await mongoDbRepository.GetFacts(new GetEntityQuery(Guid.NewGuid(), 0));

            // ASSERT

            facts.ShouldBeEmpty();
        }
    }
}
