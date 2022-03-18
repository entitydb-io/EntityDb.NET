using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Transactions.Builders;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EntityDb.Abstractions.ValueObjects;
using Xunit;

namespace EntityDb.Common.Tests.Entities;

public class EntityTests : TestsBase<Startup>
{
    public EntityTests(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    private static ITransaction BuildTransaction
    (
        IServiceScope serviceScope,
        Id entityId,
        VersionNumber from,
        VersionNumber to,
        TestEntity? entity = null
    )
    {
        var transactionBuilder = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TestEntity>>()
            .ForSingleEntity(entityId);

        if (entity != null)
        {
            transactionBuilder.Load(entity);
        }

        for (var i = from; i.Value <= to.Value; i = i.Next())
        {
            if (transactionBuilder.IsEntityKnown() && transactionBuilder.GetEntity().VersionNumber.Value >= i.Value)
            {
                continue;
            }

            transactionBuilder.Append(new DoNothing());
        }

        return transactionBuilder.Build(default!, Id.NewId());
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndSnapshots))]
    public async Task GivenEntityWithNVersions_WhenGettingAtVersionM_ThenReturnAtVersionM(TransactionsAdder transactionsAdder, SnapshotsAdder snapshotsAdder)
    {
        // ARRANGE

        const ulong n = 10UL;
        const ulong m = 5UL;
        
        var versionNumberN = new VersionNumber(n);

        var versionNumberM = new VersionNumber(m);
        
        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.Add(serviceCollection);
            snapshotsAdder.Add(serviceCollection);
        });

        var entityId = Id.NewId();

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TestEntity>>()
            .CreateRepository(TestSessionOptions.Write,
                TestSessionOptions.Write);

        var transaction = BuildTransaction(serviceScope, entityId, new VersionNumber(1), versionNumberN);

        var transactionInserted = await entityRepository.PutTransaction(transaction);

        // ARRANGE ASSERTIONS

        transactionInserted.ShouldBeTrue();

        // ACT

        var entityAtVersionM = await entityRepository.GetAtVersion(entityId, versionNumberM);

        // ASSERT

        entityAtVersionM.VersionNumber.ShouldBe(versionNumberM);
    }

    [Fact]
    public async Task GivenExistingEntityWithNoSnapshot_WhenGettingEntity_ThenGetCommandsRuns()
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
                {
                    if (transactionStep is IAppendCommandTransactionStep commandTransactionStep)
                    {
                        commands.Add(commandTransactionStep.Command);
                    }
                }

                return true;
            });

        transactionRepositoryMock
            .Setup(factory => factory.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        transactionRepositoryMock
            .Setup(repository => repository.GetCommands(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => commands.ToArray())
            .Verifiable();

        var transactionRepositoryFactoryMock =
            new Mock<ITransactionRepositoryFactory>(MockBehavior.Strict);

        transactionRepositoryFactoryMock
            .Setup(factory => factory.CreateRepository(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionRepositoryMock.Object);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(transactionRepositoryFactoryMock.Object);
        });

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TestEntity>>()
            .CreateRepository(TestSessionOptions.Write);

        var transaction = BuildTransaction(serviceScope, entityId, new VersionNumber(1), expectedVersionNumber);

        var transactionInserted = await entityRepository.PutTransaction(transaction);

        // ARRANGE ASSERTIONS

        transactionInserted.ShouldBeTrue();

        // ACT

        var currenEntity = await entityRepository.GetCurrent(entityId);

        // ASSERT

        currenEntity.VersionNumber.ShouldBe(expectedVersionNumber);

        transactionRepositoryMock
            .Verify(repository => repository.GetCommands(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenNoSnapshotRepositoryFactory_WhenCreatingEntityRepository_ThenNoSnapshotRepository()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedTransactionRepositoryFactory());
        });

        // ACT

        var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetService<ISnapshotRepositoryFactory<TestEntity>>();

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TestEntity>>()
            .CreateRepository("NOT NULL", "NOT NULL");

        // ASSERT

        snapshotRepositoryFactory.ShouldBeNull();

        entityRepository.SnapshotRepository.ShouldBeNull();
    }

    [Fact]
    public async Task GivenNoSnapshotSessionOptions_WhenCreatingEntityRepository_ThenNoSnapshotRepository()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedTransactionRepositoryFactory());
            serviceCollection.AddSingleton(GetMockedSnapshotRepositoryFactory());
        });

        // ACT

        var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetService<ISnapshotRepositoryFactory<TestEntity>>();

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TestEntity>>()
            .CreateRepository("NOT NULL");

        // ASSERT

        snapshotRepositoryFactory.ShouldNotBeNull();

        entityRepository.SnapshotRepository.ShouldBeNull();
    }

    [Fact]
    public async Task GivenSnapshotRepositoryFactoryAndSnapshotSessionOptions_WhenCreatingEntityRepository_ThenNoSnapshotRepository()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedTransactionRepositoryFactory());
            serviceCollection.AddSingleton(GetMockedSnapshotRepositoryFactory());
        });

        // ACT

        var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetService<ISnapshotRepositoryFactory<TestEntity>>();

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TestEntity>>()
            .CreateRepository("NOT NULL", "NOT NULL");

        // ASSERT

        snapshotRepositoryFactory.ShouldNotBeNull();

        entityRepository.SnapshotRepository.ShouldNotBeNull();
    }

    [Fact]
    public async Task GivenSnapshotAndNewCommands_WhenGettingSnapshotOrDefault_ThenReturnNewerThanSnapshot()
    {
        // ARRANGE

        var snapshot = new TestEntity(new VersionNumber(1));

        var newCommands = new object[]
        {
            new DoNothing(),
            new DoNothing()
        };

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedTransactionRepositoryFactory(newCommands));
            serviceCollection.AddSingleton(GetMockedSnapshotRepositoryFactory(snapshot));
        });

        // ACT

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TestEntity>>()
            .CreateRepository("NOT NULL", "NOT NULL");

        var snapshotOrDefault = await entityRepository.GetCurrent(default);

        // ASSERT

        snapshotOrDefault.ShouldNotBe(default);
        snapshotOrDefault.ShouldNotBe(snapshot);
        snapshotOrDefault.VersionNumber.ShouldBe(new VersionNumber(snapshot.VersionNumber.Value + Convert.ToUInt64(newCommands.Length)));
    }

    [Fact]
    public async Task GivenNonExistentEntityId_WhenGettingCurrentEntity_ThenThrow()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedTransactionRepositoryFactory());
        });

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TestEntity>>()
            .CreateRepository(default!);

        // ASSERT

        await Should.ThrowAsync<EntityNotCreatedException>(async () =>
        {
            await entityRepository.GetCurrent(default);
        });
    }
}