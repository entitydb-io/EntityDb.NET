using System;
using System.Reflection;
using System.Threading.Tasks;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Tests.Implementations.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Snapshots;

public sealed class SnapshotTests : TestsBase<Startup>
{
    public SnapshotTests(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
    {
    }

    private async Task GivenEmptySnapshotRepository_WhenSnapshotInsertedAndFetched_ThenInsertedMatchesFetched<TSnapshot>(SnapshotsAdder snapshotsAdder)
        where TSnapshot : ISnapshotWithTestMethods<TSnapshot>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotsAdder.Add(serviceCollection);
        });

        var snapshotId = Id.NewId();
        var expectedSnapshot = TSnapshot.Construct(snapshotId).WithVersionNumber(new VersionNumber(300));

        await using var snapshotRepositoryFactory = serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>();
        
        await using var snapshotRepository = await snapshotRepositoryFactory
            .CreateRepository(TestSessionOptions.Write);

        // ACT

        var snapshotInserted = await snapshotRepository.PutSnapshot(snapshotId, expectedSnapshot);

        var actualSnapshot = await snapshotRepository.GetSnapshotOrDefault(snapshotId);

        // ASSERT

        snapshotInserted.ShouldBeTrue();

        actualSnapshot.ShouldBeEquivalentTo(expectedSnapshot);
    }

    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    [MemberData(nameof(AddOneToOneProjectionSnapshots))]
    public async Task GivenSnapshotsAdder_WhenSnapshotInsertedAndFetched_ThenInsertedMatchesFetched(SnapshotsAdder snapshotsAdder)
    {
        await GetType()
            .GetMethod(nameof(GivenEmptySnapshotRepository_WhenSnapshotInsertedAndFetched_ThenInsertedMatchesFetched), ~BindingFlags.Public)!
            .MakeGenericMethod(snapshotsAdder.SnapshotType)
            .Invoke(this, new object?[] { snapshotsAdder })
            .ShouldBeAssignableTo<Task>().ShouldNotBeNull();
    }
    
    private async Task GivenEmptySnapshotRepository_WhenPuttingSnapshotInReadOnlyMode_ThenCannotWriteInReadOnlyModeExceptionIsLogged<TSnapshot>(SnapshotsAdder snapshotsAdder)
        where TSnapshot : ISnapshotWithTestMethods<TSnapshot>
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<CannotWriteInReadOnlyModeException>();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotsAdder.Add(serviceCollection);
            
            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactory);
        });

        var snapshot = TSnapshot.Construct(default).WithVersionNumber(new VersionNumber(300));

        await using var snapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>()
            .CreateRepository(TestSessionOptions.ReadOnly);
        
        // ACT

        var inserted = await snapshotRepository.PutSnapshot(default, snapshot);

        // ASSERT

        inserted.ShouldBeFalse();

        loggerVerifier.Invoke(Times.Once());
    }
    
    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    [MemberData(nameof(AddOneToOneProjectionSnapshots))]
    public async Task GivenSnapshotsAdder_WhenPuttingSnapshotInReadOnlyMode_ThenCannotWriteInReadOnlyModeExceptionIsLogged(SnapshotsAdder snapshotsAdder)
    {
        await GetType()
            .GetMethod(nameof(GivenEmptySnapshotRepository_WhenPuttingSnapshotInReadOnlyMode_ThenCannotWriteInReadOnlyModeExceptionIsLogged), ~BindingFlags.Public)!
            .MakeGenericMethod(snapshotsAdder.SnapshotType)
            .Invoke(this, new object?[] { snapshotsAdder })
            .ShouldBeAssignableTo<Task>().ShouldNotBeNull();
    }

    private async Task GivenInsertedSnapshot_WhenReadInVariousReadModes_ThenReturnSameSnapshot<TSnapshot>(SnapshotsAdder snapshotsAdder)
        where TSnapshot : ISnapshotWithTestMethods<TSnapshot>
    {
        // ARRANGE

        var snapshotId = Id.NewId();

        var expectedSnapshot = TSnapshot.Construct(snapshotId).WithVersionNumber(new VersionNumber(5000));

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotsAdder.Add(serviceCollection);
        });
        
        await using var writeSnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>()
            .CreateRepository(TestSessionOptions.Write);
        
        await using var readOnlySnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>()
            .CreateRepository(TestSessionOptions.ReadOnly);
        
        await using var readOnlySecondaryPreferredSnapshotRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISnapshotRepositoryFactory<TSnapshot>>()
            .CreateRepository(TestSessionOptions.ReadOnlySecondaryPreferred);
        
        var inserted = await writeSnapshotRepository.PutSnapshot(snapshotId, expectedSnapshot);

        // ARRANGE ASSERTIONS

        inserted.ShouldBeTrue();

        // ACT

        var readOnlySnapshot = await readOnlySnapshotRepository.GetSnapshotOrDefault(snapshotId);

        var readOnlySecondaryPreferredSnapshot = await readOnlySecondaryPreferredSnapshotRepository.GetSnapshotOrDefault(snapshotId);

        // ASSERT

        readOnlySnapshot.ShouldBeEquivalentTo(expectedSnapshot);
        readOnlySecondaryPreferredSnapshot.ShouldBeEquivalentTo(expectedSnapshot);
    }
    
    [Theory]
    [MemberData(nameof(AddEntitySnapshots))]
    [MemberData(nameof(AddOneToOneProjectionSnapshots))]
    public async Task GivenSnapshotsAdder_WhenReadingInsertedSnapshotInVariousReadModes_ThenReturnSameSnapshot(SnapshotsAdder snapshotsAdder)
    {
        await GetType()
            .GetMethod(nameof(GivenInsertedSnapshot_WhenReadInVariousReadModes_ThenReturnSameSnapshot), ~BindingFlags.Public)!
            .MakeGenericMethod(snapshotsAdder.SnapshotType)
            .Invoke(this, new object?[] { snapshotsAdder })
            .ShouldBeAssignableTo<Task>().ShouldNotBeNull();
    }
}