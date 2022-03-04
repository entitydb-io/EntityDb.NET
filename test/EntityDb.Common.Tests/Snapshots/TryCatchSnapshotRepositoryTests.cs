using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Tests.Implementations.Entities;
using Moq;
using Shouldly;
using System;
using System.Threading.Tasks;
using EntityDb.Abstractions.ValueObjects;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EntityDb.Common.Tests.Snapshots;

public class TryCatchSnapshotRepositoryTests : TestsBase<Startup>
{
    public TryCatchSnapshotRepositoryTests(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Fact]
    public async Task GivenRepositoryAlwaysThrows_WhenExecutingAnyMethod_ThenExceptionIsLogged()
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<Exception>();

        var snapshotRepositoryMock = new Mock<ISnapshotRepository<TransactionEntity>>(MockBehavior.Strict);

        snapshotRepositoryMock
            .Setup(repository => repository.GetSnapshot(It.IsAny<Id>()))
            .ThrowsAsync(new NotImplementedException());

        snapshotRepositoryMock
            .Setup(repository => repository.PutSnapshot(It.IsAny<Id>(), It.IsAny<TransactionEntity>()))
            .ThrowsAsync(new NotImplementedException());

        snapshotRepositoryMock
            .Setup(repository => repository.DeleteSnapshots(It.IsAny<Id[]>()))
            .ThrowsAsync(new NotImplementedException());

        var tryCatchSnapshotRepository = new TryCatchSnapshotRepository<TransactionEntity>(snapshotRepositoryMock.Object, loggerFactory.CreateLogger<object>());

        // ACT

        var snapshot = await tryCatchSnapshotRepository.GetSnapshot(default);
        var inserted = await tryCatchSnapshotRepository.PutSnapshot(default, default!);
        var deleted = await tryCatchSnapshotRepository.DeleteSnapshots(default!);

        // ASSERT

        snapshot.ShouldBe(default);
        inserted.ShouldBeFalse();
        deleted.ShouldBeFalse();

        loggerVerifier.Invoke(Times.Exactly(3));
    }
}