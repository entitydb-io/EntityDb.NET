using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Builders;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Polyfills;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Entities;

public class EntityTests : TestsBase<Startup>
{
    public EntityTests(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    private static async Task<ITransaction> BuildTransaction<TEntity>
    (
        IServiceScope serviceScope,
        Id entityId,
        VersionNumber from,
        VersionNumber to,
        TEntity? entity = default
    )
        where TEntity : IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, entityId);

        if (entity is not null) transactionBuilder.Load(entity);

        for (var i = from; i.Value <= to.Value; i = i.Next())
        {
            if (transactionBuilder.IsEntityKnown() &&
                transactionBuilder.GetEntity().VersionNumber.Value >= i.Value) continue;

            transactionBuilder.Append(new DoNothing());
        }

        return transactionBuilder.Build(Id.NewId());
    }


    private async Task Generic_GivenEntityWithNVersions_WhenGettingAtVersionM_ThenReturnAtVersionM<TEntity>(
        TransactionsAdder transactionsAdder, SnapshotAdder entitySnapshotAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        const ulong n = 10UL;
        const ulong m = 5UL;

        var versionNumberN = new VersionNumber(n);

        var versionNumberM = new VersionNumber(m);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entitySnapshotAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.Write,
                TestSessionOptions.Write);

        var transaction = await BuildTransaction<TEntity>(serviceScope, entityId, new VersionNumber(1), versionNumberN);

        var transactionInserted = await entityRepository.PutTransaction(transaction);

        // ARRANGE ASSERTIONS

        transactionInserted.ShouldBeTrue();

        // ACT

        var entityAtVersionM = await entityRepository.GetSnapshot(entityId + versionNumberM);

        // ASSERT

        entityAtVersionM.VersionNumber.ShouldBe(versionNumberM);
    }

    private async Task Generic_GivenExistingEntityWithNoSnapshot_WhenGettingEntity_ThenGetCommandsRuns<TEntity>(
        EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        var expectedVersionNumber = new VersionNumber(10);

        var entityId = Id.NewId();

        var commands = new List<object>();

        var transactionRepositoryMock = new Mock<ITransactionRepository>(MockBehavior.Strict);

        transactionRepositoryMock
            .Setup(repository => repository.PutTransaction(It.IsAny<ITransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ITransaction transaction, CancellationToken _) =>
            {
                foreach (var transactionStep in transaction.Steps)
                    if (transactionStep is IAppendCommandTransactionStep commandTransactionStep)
                        commands.Add(commandTransactionStep.Command);

                return true;
            });

        transactionRepositoryMock
            .Setup(factory => factory.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateCommands(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()))
            .Returns(() => AsyncEnumerablePolyfill.FromResult(commands))
            .Verifiable();

        var transactionRepositoryFactoryMock =
            new Mock<ITransactionRepositoryFactory>(MockBehavior.Strict);

        transactionRepositoryFactoryMock
            .Setup(factory => factory.CreateRepository(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionRepositoryMock.Object);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(transactionRepositoryFactoryMock.Object);
        });

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(TestSessionOptions.Write);

        var transaction =
            await BuildTransaction<TEntity>(serviceScope, entityId, new VersionNumber(1), expectedVersionNumber);

        var transactionInserted = await entityRepository.PutTransaction(transaction);

        // ARRANGE ASSERTIONS

        transactionInserted.ShouldBeTrue();

        // ACT

        var currenEntity = await entityRepository.GetSnapshot(entityId);

        // ASSERT

        currenEntity.VersionNumber.ShouldBe(expectedVersionNumber);

        transactionRepositoryMock
            .Verify(repository => repository.EnumerateCommands(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
    }

    private async Task
        Generic_GivenNoSnapshotRepositoryFactory_WhenCreatingEntityRepository_ThenNoSnapshotRepository<TEntity>(
            EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedTransactionRepositoryFactory());
        });

        // ACT

        await using var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetService<ISnapshotRepositoryFactory<TEntity>>();

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository("NOT NULL", "NOT NULL");

        // ASSERT

        snapshotRepositoryFactory.ShouldBeNull();

        entityRepository.SnapshotRepository.ShouldBeNull();
    }

    private async Task
        Generic_GivenNoSnapshotSessionOptions_WhenCreatingEntityRepository_ThenNoSnapshotRepository<TEntity>(
            EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedTransactionRepositoryFactory());
            serviceCollection.AddSingleton(GetMockedSnapshotRepositoryFactory<TEntity>());
        });

        // ACT

        await using var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetService<ISnapshotRepositoryFactory<TEntity>>();

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository("NOT NULL");

        // ASSERT

        snapshotRepositoryFactory.ShouldNotBeNull();

        entityRepository.SnapshotRepository.ShouldBeNull();
    }

    private async Task
        Generic_GivenSnapshotRepositoryFactoryAndSnapshotSessionOptions_WhenCreatingEntityRepository_ThenNoSnapshotRepository<
            TEntity>(EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedTransactionRepositoryFactory());
            serviceCollection.AddSingleton(GetMockedSnapshotRepositoryFactory<TEntity>());
        });

        // ACT

        await using var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetService<ISnapshotRepositoryFactory<TEntity>>();

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository("NOT NULL", "NOT NULL");

        // ASSERT

        snapshotRepositoryFactory.ShouldNotBeNull();

        entityRepository.SnapshotRepository.ShouldNotBeNull();
    }

    private async Task
        Generic_GivenSnapshotAndNewCommands_WhenGettingSnapshotOrDefault_ThenReturnNewerThanSnapshot<TEntity>(
            EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        var snapshot = TEntity.Construct(default).WithVersionNumber(new VersionNumber(1));

        var newCommands = new object[]
        {
            new DoNothing(),
            new DoNothing()
        };

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedTransactionRepositoryFactory(newCommands));
            serviceCollection.AddSingleton(GetMockedSnapshotRepositoryFactory(snapshot));
        });

        // ACT

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository("NOT NULL", "NOT NULL");

        var snapshotOrDefault = await entityRepository.GetSnapshot(default);

        // ASSERT

        snapshotOrDefault.ShouldNotBe(default);
        snapshotOrDefault.ShouldNotBe(snapshot);
        snapshotOrDefault.VersionNumber.ShouldBe(
            new VersionNumber(snapshot.VersionNumber.Value + Convert.ToUInt64(newCommands.Length)));
    }

    private async Task Generic_GivenNonExistentEntityId_WhenGettingCurrentEntity_ThenThrow<TEntity>(
        EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddSingleton(GetMockedTransactionRepositoryFactory());
        });

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TEntity>>()
            .CreateRepository(default!);

        // ASSERT

        await Should.ThrowAsync<SnapshotPointerDoesNotExistException>(async () =>
        {
            await entityRepository.GetSnapshot(default);
        });
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntitySnapshots))]
    public Task GivenEntityWithNVersions_WhenGettingAtVersionM_ThenReturnAtVersionM(TransactionsAdder transactionsAdder,
        SnapshotAdder entitySnapshotAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entitySnapshotAdder.SnapshotType },
            new object?[] { transactionsAdder, entitySnapshotAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenExistingEntityWithNoSnapshot_WhenGettingEntity_ThenGetCommandsRuns(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenNoSnapshotRepositoryFactory_WhenCreatingEntityRepository_ThenNoSnapshotRepository(
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenNoSnapshotSessionOptions_WhenCreatingEntityRepository_ThenNoSnapshotRepository(
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task
        GivenSnapshotRepositoryFactoryAndSnapshotSessionOptions_WhenCreatingEntityRepository_ThenNoSnapshotRepository(
            EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenSnapshotAndNewCommands_WhenGettingSnapshotOrDefault_ThenReturnNewerThanSnapshot(
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddEntity))]
    public Task GivenNonExistentEntityId_WhenGettingCurrentEntity_ThenThrow(EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { entityAdder }
        );
    }
}