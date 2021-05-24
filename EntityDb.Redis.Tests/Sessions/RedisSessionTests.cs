using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using EntityDb.Redis.Snapshots;
using EntityDb.TestImplementations.Entities;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Redis.Tests.Sessions
{
    public class RedisSessionTests
    {
        private readonly IServiceProvider _serviceProvider;

        public RedisSessionTests(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [Fact]
        public async Task GivenValidRedisSession_WhenThrowingDuringExecuteQuery_ThenReturnDefault()
        {
            var snapshotRepositoryFactory = await _serviceProvider.CreateSnapshotRepository<TransactionEntity>(new SnapshotSessionOptions());

            if (snapshotRepositoryFactory is RedisSnapshotRepository<TransactionEntity> redisSnapshotRepository)
            {
                // ARRANGE

                var redisSession = redisSnapshotRepository.RedisSession;

                // ACT

                var result = await redisSession.ExecuteQuery<object?>((serviceProvider, redisDatabase) => throw new Exception(), default);

                // ASSERT

                Assert.Null(result);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }


        [Fact]
        public async Task GivenValidRedisSession_WhenThrowingDuringExecuteComand_ThenReturnFalse()
        {
            var snapshotRepositoryFactory = await _serviceProvider.CreateSnapshotRepository<TransactionEntity>(new SnapshotSessionOptions());

            if (snapshotRepositoryFactory is RedisSnapshotRepository<TransactionEntity> redisSnapshotRepository)
            {
                // ARRANGE

                var redisSession = redisSnapshotRepository.RedisSession;

                // ACT

                var executed = await redisSession.ExecuteCommand((serviceProvider, redisTransaction) => throw new Exception());

                // ASSERT

                Assert.False(executed);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
