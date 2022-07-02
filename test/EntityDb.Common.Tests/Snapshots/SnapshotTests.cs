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

    private async Task GivenEmptySnapshotRepository_WhenSnapshotInsertedAndFetched_ThenInsertedMatchesFetched<TSnapshot>(SnapshotAdder snapshotAdder)
        where TSnapshot : ISnapshotWithTestLogic<TSnapshot>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotAdder.AddDependencies.Invoke(serviceCollection);
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
    [MemberData(nameof(AddProjectionSnapshots))]
    public Task GivenSnapshotsAdder_WhenSnapshotInsertedAndFetched_ThenInsertedMatchesFetched(SnapshotAdder snapshotAdder)
    {
        return RunGenericTestAsync
        (
            nameof(GivenEmptySnapshotRepository_WhenSnapshotInsertedAndFetched_ThenInsertedMatchesFetched),
            new[] { snapshotAdder.SnapshotType },
            new object?[] { snapshotAdder }
        );
    }
    
    private async Task GivenEmptySnapshotRepository_WhenPuttingSnapshotInReadOnlyMode_ThenCannotWriteInReadOnlyModeExceptionIsLogged<TSnapshot>(SnapshotAdder snapshotAdder)
        where TSnapshot : ISnapshotWithTestLogic<TSnapshot>
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<CannotWriteInReadOnlyModeException>();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotAdder.AddDependencies.Invoke(serviceCollection);
            
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
    [MemberData(nameof(AddProjectionSnapshots))]
    public Task GivenSnapshotsAdder_WhenPuttingSnapshotInReadOnlyMode_ThenCannotWriteInReadOnlyModeExceptionIsLogged(SnapshotAdder snapshotAdder)
    {
        return RunGenericTestAsync
        (
            nameof(GivenEmptySnapshotRepository_WhenPuttingSnapshotInReadOnlyMode_ThenCannotWriteInReadOnlyModeExceptionIsLogged),
            new[] { snapshotAdder.SnapshotType },
            new object?[] { snapshotAdder }
        );
    }

    private async Task GivenInsertedSnapshot_WhenReadInVariousReadModes_ThenReturnSameSnapshot<TSnapshot>(SnapshotAdder snapshotAdder)
        where TSnapshot : ISnapshotWithTestLogic<TSnapshot>
    {
        // ARRANGE

        var snapshotId = Id.NewId();

        var expectedSnapshot = TSnapshot.Construct(snapshotId).WithVersionNumber(new VersionNumber(5000));

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            snapshotAdder.AddDependencies.Invoke(serviceCollection);
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
    [MemberData(nameof(AddProjectionSnapshots))]
    public Task GivenSnapshotsAdder_WhenReadingInsertedSnapshotInVariousReadModes_ThenReturnSameSnapshot(SnapshotAdder snapshotAdder)
    {
        return RunGenericTestAsync
        (
            nameof(GivenInsertedSnapshot_WhenReadInVariousReadModes_ThenReturnSameSnapshot),
            new[] { snapshotAdder.SnapshotType },
            new object?[] { snapshotAdder }
        );
    }
}