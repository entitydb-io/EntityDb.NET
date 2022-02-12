using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Tests.Implementations.Entities;
using Moq;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Common.Tests.Transactions
{
    public class TryCatchSnapshotRepositoryTests : TestsBase<Startup>
    {
        public TryCatchSnapshotRepositoryTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [Fact]
        public async Task GivenRepositoryAlwaysThrows_WhenExecutingAnyMethod_ThenExceptionIsLogged()
        {
            // ARRANGE

            var loggerMock = new Mock<ILogger>(MockBehavior.Strict);

            loggerMock
                .Setup(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()))
                .Verifiable();

            var snapshotRepositoryMock = new Mock<ISnapshotRepository<TransactionEntity>>(MockBehavior.Strict);

            snapshotRepositoryMock
                .Setup(repository => repository.GetSnapshot(It.IsAny<Guid>()))
                .ThrowsAsync(new NotImplementedException());

            snapshotRepositoryMock
                .Setup(repository => repository.PutSnapshot(It.IsAny<Guid>(), It.IsAny<TransactionEntity>()))
                .ThrowsAsync(new NotImplementedException());

            snapshotRepositoryMock
                .Setup(repository => repository.DeleteSnapshots(It.IsAny<Guid[]>()))
                .ThrowsAsync(new NotImplementedException());

            var tryCatchSnapshotRepository = new TryCatchSnapshotRepository<TransactionEntity>(snapshotRepositoryMock.Object, loggerMock.Object);

            // ACT

            var snapshot = await tryCatchSnapshotRepository.GetSnapshot(default);
            var inserted = await tryCatchSnapshotRepository.PutSnapshot(default, default!);
            var deleted = await tryCatchSnapshotRepository.DeleteSnapshots(default!);

            // ASSERT

            snapshot.ShouldBe(default);
            inserted.ShouldBeFalse();
            deleted.ShouldBeFalse();

            loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Exactly(3));
        }
    }
}
