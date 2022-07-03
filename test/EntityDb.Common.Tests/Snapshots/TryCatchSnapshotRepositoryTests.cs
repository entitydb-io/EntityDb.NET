using System;
using System.Threading;
using System.Threading.Tasks;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Snapshots;

public class TryCatchSnapshotRepositoryTests : TestsBase<Startup>
{
    public TryCatchSnapshotRepositoryTests(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    private async Task Generic_GivenRepositoryAlwaysThrows_WhenExecutingAnyMethod_ThenExceptionIsLogged<TEntity>(
        EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<Exception>();

        var snapshotRepositoryMock = new Mock<ISnapshotRepository<TEntity>>(MockBehavior.Strict);

        snapshotRepositoryMock
            .Setup(repository => repository.GetSnapshotOrDefault(It.IsAny<Pointer>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        snapshotRepositoryMock
            .Setup(repository =>
                repository.PutSnapshot(It.IsAny<Pointer>(), It.IsAny<TEntity>(), It.IsAny<CancellationToken>()))
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

        var tryCatchSnapshotRepository = TryCatchSnapshotRepository<TEntity>
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