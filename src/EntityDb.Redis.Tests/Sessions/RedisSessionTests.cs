using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
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
        private readonly ISnapshotRepositoryFactory<TransactionEntity> _snapshotRepositoryFactory;

        public RedisSessionTests(ISnapshotRepositoryFactory<TransactionEntity> snapshotRepositoryFactory)
        {
            _snapshotRepositoryFactory = snapshotRepositoryFactory;
        }

        [Fact]
        public async Task GivenValidRedisSession_WhenThrowingDuringExecuteQuery_ThenReturnDefault()
        {
            var snapshotRepository =
                await _snapshotRepositoryFactory.CreateRepository(new SnapshotSessionOptions());

            if (snapshotRepository is RedisSnapshotRepository<TransactionEntity> redisSnapshotRepository)
            {
                // ARRANGE

                var redisSession = redisSnapshotRepository.RedisSession;

                // ACT

                var result =
                    await redisSession.ExecuteQuery<object?>(
                        (_, _, _) => throw new Exception(), default);

                // ASSERT

                result.ShouldBeNull();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }


        [Fact]
        public async Task GivenValidRedisSession_WhenThrowingDuringExecuteCommand_ThenReturnFalse()
        {
            var snapshotRepository =
                await _snapshotRepositoryFactory.CreateRepository(new SnapshotSessionOptions());

            if (snapshotRepository is RedisSnapshotRepository<TransactionEntity> redisSnapshotRepository)
            {
                // ARRANGE

                var redisSession = redisSnapshotRepository.RedisSession;

                // ACT

                var executed =
                    await redisSession.ExecuteCommand((_, _) => throw new Exception());

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
