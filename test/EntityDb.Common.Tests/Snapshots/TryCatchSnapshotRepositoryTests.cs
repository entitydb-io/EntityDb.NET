using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Tests.Implementations.Entities;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using EntityDb.Abstractions.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        var snapshotRepositoryMock = new Mock<ISnapshotRepository<TestEntity>>(MockBehavior.Strict);

        snapshotRepositoryMock
            .Setup(repository => repository.GetSnapshot(It.IsAny<Id>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        snapshotRepositoryMock
            .Setup(repository => repository.PutSnapshot(It.IsAny<Id>(), It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        snapshotRepositoryMock
            .Setup(repository => repository.DeleteSnapshots(It.IsAny<Id[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactory);
        });

        var tryCatchSnapshotRepository = TryCatchSnapshotRepository<TestEntity>
            .Create(serviceScope.ServiceProvider, snapshotRepositoryMock.Object);

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