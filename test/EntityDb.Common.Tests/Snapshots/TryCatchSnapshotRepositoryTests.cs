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
using EntityDb.Common.Entities;

namespace EntityDb.Common.Tests.Snapshots;

public class TryCatchSnapshotRepositoryTests : TestsBase<Startup>
{
    public TryCatchSnapshotRepositoryTests(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    private async Task Generic_GivenRepositoryAlwaysThrows_WhenExecutingAnyMethod_ThenExceptionIsLogged<TEntity>(EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<Exception>();

        var snapshotRepositoryMock = new Mock<ISnapshotRepository<TestEntity>>(MockBehavior.Strict);

        snapshotRepositoryMock
            .Setup(repository => repository.GetSnapshotOrDefault(It.IsAny<Pointer>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        snapshotRepositoryMock
            .Setup(repository => repository.PutSnapshot(It.IsAny<Pointer>(), It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        snapshotRepositoryMock
            .Setup(repository => repository.DeleteSnapshots(It.IsAny<Pointer[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactory);
        });

        var tryCatchSnapshotRepository = TryCatchSnapshotRepository<TestEntity>
            .Create(serviceScope.ServiceProvider, snapshotRepositoryMock.Object);

        // ACT

        var snapshot = await tryCatchSnapshotRepository.GetSnapshotOrDefault(default);
        var inserted = await tryCatchSnapshotRepository.PutSnapshot(default, default!);
        var deleted = await tryCatchSnapshotRepository.DeleteSnapshots(default!);

        // ASSERT

        snapshot.ShouldBe(default);
        inserted.ShouldBeFalse();
        deleted.ShouldBeFalse();

        loggerVerifier.Invoke(Times.Exactly(3));
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenRepositoryAlwaysThrows_WhenExecutingAnyMethod_ThenExceptionIsLogged(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }
}