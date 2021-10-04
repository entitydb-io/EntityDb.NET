using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using EntityDb.Redis.Sessions;
using EntityDb.Redis.Snapshots;
using EntityDb.TestImplementations.Entities;
using Shouldly;
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
            ISnapshotRepository<TransactionEntity>? snapshotRepositoryFactory =
                await _serviceProvider.CreateSnapshotRepository<TransactionEntity>(new SnapshotSessionOptions());

            if (snapshotRepositoryFactory is RedisSnapshotRepository<TransactionEntity> redisSnapshotRepository)
            {
                // ARRANGE

                IRedisSession? redisSession = redisSnapshotRepository.RedisSession;

                // ACT

                object? result =
                    await redisSession.ExecuteQuery<object?>(
                        (logger, resolvingStrategyChain, redisDatabase) => throw new Exception(), default);

                // ASSERT

                result.ShouldBeNull();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }


        [Fact]
        public async Task GivenValidRedisSession_WhenThrowingDuringExecuteComand_ThenReturnFalse()
        {
            ISnapshotRepository<TransactionEntity>? snapshotRepositoryFactory =
                await _serviceProvider.CreateSnapshotRepository<TransactionEntity>(new SnapshotSessionOptions());

            if (snapshotRepositoryFactory is RedisSnapshotRepository<TransactionEntity> redisSnapshotRepository)
            {
                // ARRANGE

                IRedisSession? redisSession = redisSnapshotRepository.RedisSession;

                // ACT

                bool executed =
                    await redisSession.ExecuteCommand((serviceProvider, redisTransaction) => throw new Exception());

                // ASSERT

                executed.ShouldBeFalse();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
