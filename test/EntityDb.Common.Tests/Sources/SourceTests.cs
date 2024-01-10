using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Sources.Agents;
using EntityDb.Common.Sources.Attributes;
using EntityDb.Common.Sources.Queries;
using EntityDb.Common.Sources.Queries.Modified;
using EntityDb.Common.Sources.Queries.Standard;
using EntityDb.Common.Tests.Implementations.Deltas;
using EntityDb.Common.Tests.Implementations.Leases;
using EntityDb.Common.Tests.Implementations.Queries;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.Common.Tests.Implementations.Tags;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.Sources;

[Collection(nameof(DatabaseContainerCollection))]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class SourceTests : TestsBase<Startup>
{
    public SourceTests(IServiceProvider startupServiceProvider, DatabaseContainerFixture databaseContainerFixture)
        : base(startupServiceProvider, databaseContainerFixture)
    {
    }

    private static async Task PutSources
    (
        IServiceScope serviceScope,
        List<Source> sources
    )
    {
        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        foreach (var source in sources)
        {
            var sourceCommitted = await sourceRepository.Commit(source);

            sourceCommitted.ShouldBeTrue();
        }
    }

    private static ModifiedQueryOptions NewModifiedQueryOptions(bool invertFilter, bool reverseSort, int? replaceSkip,
        int? replaceTake)
    {
        return new ModifiedQueryOptions
        {
            InvertFilter = invertFilter,
            ReverseSort = reverseSort,
            ReplaceSkip = replaceSkip,
            ReplaceTake = replaceTake,
        };
    }

    private static async Task TestGet<TResult>
    (
        IServiceScope serviceScope,
        Func<bool, TResult[]> getExpectedResults,
        Func<ISourceRepository, ModifiedQueryOptions, IAsyncEnumerable<TResult>> getActualResults,
        bool secondaryPreferred
    )
    {
        // ARRANGE

        var bufferModifier = NewModifiedQueryOptions(false, false, null, null);
        var negateModifier = NewModifiedQueryOptions(true, false, null, null);
        var reverseBufferModifier = NewModifiedQueryOptions(false, true, null, null);
        var reverseNegateModifier = NewModifiedQueryOptions(true, true, null, null);
        var bufferSubsetModifier = NewModifiedQueryOptions(false, false, 1, 1);

        var expectedTrueResults = getExpectedResults.Invoke(false);
        var expectedFalseResults = getExpectedResults.Invoke(true);
        var reversedExpectedTrueResults = expectedTrueResults.Reverse().ToArray();
        var reversedExpectedFalseResults = expectedFalseResults.Reverse().ToArray();
        var expectedSkipTakeResults = expectedTrueResults.Skip(1).Take(1).ToArray();

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .CreateRepository(secondaryPreferred
                ? TestSessionOptions.ReadOnlySecondaryPreferred
                : TestSessionOptions.ReadOnly);

        // ACT

        var actualTrueResults =
            await getActualResults.Invoke(sourceRepository, bufferModifier).ToArrayAsync();
        var actualFalseResults =
            await getActualResults.Invoke(sourceRepository, negateModifier).ToArrayAsync();
        var reversedActualTrueResults =
            await getActualResults.Invoke(sourceRepository, reverseBufferModifier).ToArrayAsync();
        var reversedActualFalseResults =
            await getActualResults.Invoke(sourceRepository, reverseNegateModifier).ToArrayAsync();
        var actualSkipTakeResults =
            await getActualResults.Invoke(sourceRepository, bufferSubsetModifier).ToArrayAsync();

        // ASSERT

        actualTrueResults.ShouldBeEquivalentTo(expectedTrueResults);
        actualFalseResults.ShouldBeEquivalentTo(expectedFalseResults);
        reversedActualTrueResults.ShouldBeEquivalentTo(reversedExpectedTrueResults);
        reversedActualFalseResults.ShouldBeEquivalentTo(reversedExpectedFalseResults);
        actualSkipTakeResults.ShouldBeEquivalentTo(expectedSkipTakeResults);
    }

    private static async Task TestGetSourceIds
    (
        IServiceScope serviceScope,
        IMessageGroupQuery query,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseSourceIds
                    : expectedObjects.TrueSourceIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository.EnumerateSourceIds(query.Modify(modifiedQueryOptions));
        }
    }

    private static async Task TestGetSourceIds
    (
        IServiceScope serviceScope,
        IMessageQuery query,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository.EnumerateSourceIds(query.Modify(modifiedQueryOptions));
        }

        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseSourceIds
                    : expectedObjects.TrueSourceIds)
                .ToArray();
        }
    }

    private static async Task TestGetSourceIds
    (
        IServiceScope serviceScope,
        ILeaseQuery query,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseSourceIds
                    : expectedObjects.TrueSourceIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository.EnumerateSourceIds(query.Modify(modifiedQueryOptions));
        }
    }

    private static async Task TestGetSourceIds
    (
        IServiceScope serviceScope,
        ITagQuery query,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseSourceIds
                    : expectedObjects.TrueSourceIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository.EnumerateSourceIds(query.Modify(modifiedQueryOptions));
        }
    }

    private static async Task TestGetEntityIds
    (
        IServiceScope serviceScope,
        IMessageGroupQuery query,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseEntityIds
                    : expectedObjects.TrueEntityIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository
                .EnumerateEntityPointers(query.Modify(modifiedQueryOptions))
                .Select(pointer => pointer.Id);
        }
    }

    private static async Task TestGetEntityIds
    (
        IServiceScope serviceScope,
        IMessageQuery query,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseEntityIds
                    : expectedObjects.TrueEntityIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository
                .EnumerateEntityPointers(query.Modify(modifiedQueryOptions))
                .Select(pointer => pointer.Id);
        }
    }

    private static async Task TestGetEntityIds
    (
        IServiceScope serviceScope,
        ILeaseQuery query,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseEntityIds
                    : expectedObjects.TrueEntityIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository
                .EnumerateEntityPointers(query.Modify(modifiedQueryOptions))
                .Select(pointer => pointer.Id);
        }
    }

    private static async Task TestGetEntityIds
    (
        IServiceScope serviceScope,
        ITagQuery query,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseEntityIds
                    : expectedObjects.TrueEntityIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository
                .EnumerateEntityPointers(query.Modify(modifiedQueryOptions))
                .Select(pointer => pointer.Id);
        }
    }

    private static async Task TestGetAgentSignatures
    (
        IServiceScope serviceScope,
        IMessageGroupQuery query,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        object[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseAgentSignatures
                    : expectedObjects.TrueAgentSignatures)
                .ToArray();
        }

        IAsyncEnumerable<object> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository.EnumerateAgentSignatures(query.Modify(modifiedQueryOptions));
        }
    }

    private static async Task TestGetDeltas
    (
        IServiceScope serviceScope,
        IMessageQuery query,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        object[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseDeltas
                    : expectedObjects.TrueDeltas)
                .ToArray();
        }

        IAsyncEnumerable<object> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository.EnumerateDeltas(query.Modify(modifiedQueryOptions));
        }
    }

    private static async Task TestGetLeases
    (
        IServiceScope serviceScope,
        ILeaseQuery query,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        ILease[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseLeases
                    : expectedObjects.TrueLeases)
                .ToArray();
        }

        IAsyncEnumerable<ILease> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository.EnumerateLeases(query.Modify(modifiedQueryOptions));
        }
    }

    private static async Task TestGetTags
    (
        IServiceScope serviceScope,
        ITagQuery query,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        IAsyncEnumerable<ITag> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository.EnumerateTags(query.Modify(modifiedQueryOptions));
        }

        ITag[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseTags
                    : expectedObjects.TrueTags)
                .ToArray();
        }
    }

    private static async Task<Source> BuildSource<TEntity>
    (
        IServiceScope serviceScope,
        Id sourceId,
        Id entityId,
        IEnumerable<ulong> counts,
        TimeStamp? timeStampOverride = null,
        object? agentSignatureOverride = null
    )
        where TEntity : IEntity<TEntity>
    {
        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, entityId);

        foreach (var count in counts) sourceBuilder.Append(new StoreNumber(count));

        var source = sourceBuilder.Build(sourceId).ShouldBeOfType<Source>();

        if (timeStampOverride.HasValue)
            source = source with
            {
                TimeStamp = timeStampOverride.Value,
            };

        if (agentSignatureOverride is not null)
            source = source with
            {
                AgentSignature = agentSignatureOverride,
            };

        return source;
    }

    private static Id[] GetSortedIds(int numberOfIds)
    {
        return Enumerable
            .Range(1, numberOfIds)
            .Select(_ => Id.NewId())
            .OrderBy(id => id.Value)
            .ToArray();
    }

    private async Task
        Generic_GivenReadOnlyMode_WhenCommittingSource_ThenCannotWriteInReadOnlyModeExceptionIsLogged<TEntity>(
            SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<CannotWriteInReadOnlyModeException>();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactory);
        });

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var source = sourceBuilder
            .Append(DeltaSeeder.Create())
            .Build(default);

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .CreateRepository(TestSessionOptions.ReadOnly);

        // ACT

        var committed = await sourceRepository.Commit(source);

        // ASSERT

        committed.ShouldBeFalse();

        loggerVerifier.Invoke(Times.Once());
    }

    private async Task Generic_GivenNonUniqueSourceIds_WhenCommittingSources_ThenSecondPutReturnsFalse<TEntity>(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var sourceId = Id.NewId();

        var firstSourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var secondSourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var firstSource = firstSourceBuilder
            .Append(DeltaSeeder.Create())
            .Build(sourceId);

        var secondSource = secondSourceBuilder
            .Append(DeltaSeeder.Create())
            .Build(sourceId);

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        // ACT

        var firstSourceCommitted = await sourceRepository.Commit(firstSource);
        var secondSourceCommitted = await sourceRepository.Commit(secondSource);

        // ASSERT

        firstSourceCommitted.ShouldBeTrue();
        secondSourceCommitted.ShouldBeFalse();
    }

    private async Task Generic_GivenNonUniqueVersions_WhenCommittingDeltas_ThenReturnFalse<TEntity>(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        const int repeatCount = 2;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var source = sourceBuilder
            .Append(DeltaSeeder.Create())
            .Build(default)
            .ShouldBeOfType<Source>();

        source = source with
        {
            Messages = Enumerable
                .Repeat(source.Messages, repeatCount)
                .SelectMany(messages => messages)
                .ToImmutableArray(),
        };

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        // ARRANGE ASSERTIONS

        repeatCount.ShouldBeGreaterThan(1);

        // ACT

        var sourceCommitted = await sourceRepository.Commit(source);

        // ASSERT

        sourceCommitted.ShouldBeFalse();
    }

    private async Task
        Generic_GivenVersionZero_WhenCommittingDeltas_ThenReturnTrue<TEntity>(
            SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var message = new Message
        {
            Id = Id.NewId(),
            EntityPointer = Id.NewId() + Version.Zero,
            Delta = new DoNothing(),
        };

        var source = SourceSeeder.Create(message);

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        // ACT

        var sourceCommitted =
            await sourceRepository.Commit(source);

        // ASSERT

        sourceCommitted.ShouldBeTrue();
    }

    private async Task
        Generic_GivenNonUniqueVersions_WhenCommittingDeltas_ThenOptimisticConcurrencyExceptionIsLogged<TEntity>(
            SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<OptimisticConcurrencyException>();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactory);
        });

        var entityId = Id.NewId();

        var firstSourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, entityId);

        var secondSourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, entityId);

        var firstSource = firstSourceBuilder
            .Append(DeltaSeeder.Create())
            .Build(Id.NewId());

        var secondSource = secondSourceBuilder
            .Append(DeltaSeeder.Create())
            .Build(Id.NewId());

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        // ACT

        var firstSourceCommitted =
            await sourceRepository.Commit(firstSource);
        var secondSourceCommitted =
            await sourceRepository.Commit(secondSource);

        // ASSERT

        firstSource.Messages.Length.ShouldBe(1);
        secondSource.Messages.Length.ShouldBe(1);

        firstSource.Messages.ShouldAllBe(message => message.EntityPointer.Id == entityId);
        secondSource.Messages.ShouldAllBe(message => message.EntityPointer.Id == entityId);

        firstSource.Messages[0].EntityPointer.Version
            .ShouldBe(secondSource.Messages[0].EntityPointer.Version);

        firstSourceCommitted.ShouldBeTrue();
        secondSourceCommitted.ShouldBeFalse();

        loggerVerifier.Invoke(Times.Once());
    }

    private async Task Generic_GivenNonUniqueTags_WhenCommittingTags_ThenReturnTrue<TEntity>(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var tag = TagSeeder.Create();

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var source = sourceBuilder
            .Append(new AddTag(tag))
            .Append(new AddTag(tag))
            .Build(default);

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        // ACT

        var sourceCommitted = await sourceRepository.Commit(source);

        // ASSERT

        sourceCommitted.ShouldBeTrue();
    }

    private async Task Generic_GivenNonUniqueLeases_WhenCommittingLeases_ThenReturnFalse<TEntity>(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var lease = LeaseSeeder.Create();

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var source = sourceBuilder
            .Append(new AddLease(lease))
            .Append(new AddLease(lease))
            .Build(default);

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        // ACT

        var sourceCommitted = await sourceRepository.Commit(source);

        // ASSERT

        sourceCommitted.ShouldBeFalse();
    }

    private async Task
        Generic_GivenDeltaCommitted_WhenGettingAnnotatedAgentSignature_ThenReturnAnnotatedAgentSignature<TEntity>(
            SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        const ulong expectedCount = 5;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var expectedSourceId = Id.NewId();
        var expectedEntityId = Id.NewId();
        var expectedSourceTimeStamp = sourcesAdder.FixTimeStamp(TimeStamp.UtcNow);

        var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        var source = await BuildSource<TEntity>(serviceScope, expectedSourceId, expectedEntityId,
            new[] { expectedCount }, expectedSourceTimeStamp, agentSignature);

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        var sourceCommitted = await sourceRepository.Commit(source);

        var query = new EntityQuery(expectedEntityId);

        // ARRANGE ASSERTIONS

        sourceCommitted.ShouldBeTrue();

        // ACT

        var annotatedAgentSignatures =
            await sourceRepository.EnumerateAnnotatedAgentSignatures(query).ToArrayAsync();

        // ASSERT

        annotatedAgentSignatures.Length.ShouldBe(1);

        annotatedAgentSignatures[0].SourceId.ShouldBe(expectedSourceId);
        annotatedAgentSignatures[0].SourceTimeStamp.ShouldBe(expectedSourceTimeStamp);
        annotatedAgentSignatures[0].EntityPointers.Length.ShouldBe(1);
        annotatedAgentSignatures[0].EntityPointers[0].Id.ShouldBe(expectedEntityId);
        annotatedAgentSignatures[0].Data.ShouldBeEquivalentTo(agentSignature);
    }

    private async Task Generic_GivenDeltaCommitted_WhenGettingAnnotatedDeltas_ThenReturnAnnotatedDelta<TEntity>(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        const ulong expectedDeltaCount = 5;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var expectedSourceId = Id.NewId();
        var expectedEntityId = Id.NewId();
        var expectedSourceTimeStamp = sourcesAdder.FixTimeStamp(TimeStamp.UtcNow);

        var source = await BuildSource<TEntity>(serviceScope, expectedSourceId, expectedEntityId,
            new[] { expectedDeltaCount }, expectedSourceTimeStamp);

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        var sourceCommitted = await sourceRepository.Commit(source);

        var query = new GetDeltasQuery(expectedEntityId, default);

        // ARRANGE ASSERTIONS

        sourceCommitted.ShouldBeTrue();

        // ACT

        var annotatedDeltas = await sourceRepository.EnumerateAnnotatedDeltas(query).ToArrayAsync();

        // ASSERT

        annotatedDeltas.Length.ShouldBe(1);

        annotatedDeltas[0].SourceId.ShouldBe(expectedSourceId);
        annotatedDeltas[0].SourceTimeStamp.ShouldBe(expectedSourceTimeStamp);
        annotatedDeltas[0].EntityPointer.Id.ShouldBe(expectedEntityId);
        annotatedDeltas[0].EntityPointer.Version.ShouldBe(new Version(1));

        var actualDeltaCount = annotatedDeltas[0].Data.ShouldBeAssignableTo<StoreNumber>().ShouldNotBeNull();

        actualDeltaCount.Number.ShouldBe(expectedDeltaCount);
    }

    private async Task Generic_GivenEntityCommitted_WhenGettingEntity_ThenReturnEntity<TEntity>(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        var expectedEntity = TEntity.Construct(entityId).WithVersion(new Version(1));

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        var entityRepository = EntityRepository<TEntity>.Create(serviceScope.ServiceProvider, sourceRepository);

        var source = await BuildSource<TEntity>(serviceScope, Id.NewId(), entityId,
            new[] { 0UL });

        var sourceCommitted = await sourceRepository.Commit(source);

        // ARRANGE ASSERTIONS

        sourceCommitted.ShouldBeTrue();

        // ACT

        var actualEntity = await entityRepository.GetSnapshot(entityId);

        // ASSERT

        actualEntity.ShouldBeEquivalentTo(expectedEntity);
    }

    private async Task Generic_GivenEntityCommittedWithTags_WhenRemovingAllTags_ThenFinalEntityHasNoTags<TEntity>(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, entityId);

        var tag = new Tag("Foo", "Bar");

        var expectedInitialTags = new[] { tag }.ToImmutableArray<ITag>();

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        var initialSource = sourceBuilder
            .Append(new AddTag(tag))
            .Build(Id.NewId());

        var initialSourceCommitted = await sourceRepository.Commit(initialSource);

        var tagQuery = new DeleteTagsQuery(entityId, expectedInitialTags);

        // ARRANGE ASSERTIONS

        initialSourceCommitted.ShouldBeTrue();

        // ACT

        var actualInitialTags = await sourceRepository.EnumerateTags(tagQuery).ToArrayAsync();

        var finalSource = sourceBuilder
            .Append(new DeleteTag(tag))
            .Build(Id.NewId());

        var finalSourceCommitted = await sourceRepository.Commit(finalSource);

        var actualFinalTags = await sourceRepository.EnumerateTags(tagQuery).ToArrayAsync();

        // ASSERT

        finalSourceCommitted.ShouldBeTrue();

        expectedInitialTags.SequenceEqual(actualInitialTags).ShouldBeTrue();

        actualFinalTags.ShouldBeEmpty();
    }

    private async Task Generic_GivenEntityCommittedWithLeases_WhenRemovingAllLeases_ThenFinalEntityHasNoLeases<TEntity>(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, entityId);

        var lease = new Lease("Foo", "Bar", "Baz");

        var expectedInitialLeases = new[] { lease }.ToImmutableArray<ILease>();

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        var initialSource = sourceBuilder
            .Append(new AddLease(lease))
            .Build(Id.NewId());

        var initialSourceCommitted = await sourceRepository.Commit(initialSource);

        var leaseQuery = new DeleteLeasesQuery(expectedInitialLeases);

        // ARRANGE ASSERTIONS

        initialSourceCommitted.ShouldBeTrue();

        // ACT

        var actualInitialLeases = await sourceRepository.EnumerateLeases(leaseQuery).ToArrayAsync();

        var finalSource = sourceBuilder
            .Append(new DeleteLease(lease))
            .Build(Id.NewId());

        var finalSourceCommitted = await sourceRepository.Commit(finalSource);

        var actualFinalLeases = await sourceRepository.EnumerateLeases(leaseQuery).ToArrayAsync();

        // ASSERT

        finalSourceCommitted.ShouldBeTrue();

        actualInitialLeases.SequenceEqual(expectedInitialLeases).ShouldBeTrue();

        actualFinalLeases.ShouldBeEmpty();
    }

    private async Task
        Generic_GivenSourceCreatesEntity_WhenQueryingForVersionOne_ThenReturnTheExpectedDelta<TEntity>(
            SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var expectedDelta = new StoreNumber(1);

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var source = sourceBuilder
            .Append(expectedDelta)
            .Build(Id.NewId());

        var versionOneQuery = new EntityVersionQuery(new Version(1), new Version(1));

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        // ACT

        var sourceCommitted = await sourceRepository.Commit(source);

        var newDeltas = await sourceRepository.EnumerateDeltas(versionOneQuery).ToArrayAsync();

        // ASSERT

        sourceCommitted.ShouldBeTrue();

        source.Messages.Length.ShouldBe(1);

        source.Messages[0].EntityPointer.Version.ShouldBe(new Version(1));

        newDeltas.Length.ShouldBe(1);

        newDeltas[0].ShouldBeEquivalentTo(expectedDelta);
    }

    private async Task
        Generic_GivenSourceAppendsEntityWithOneVersion_WhenQueryingForVersionTwo_ThenReturnExpectedDelta<
            TEntity>(SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var expectedDelta = new StoreNumber(2);

        var sourceBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<IEntitySourceBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var firstSource = sourceBuilder
            .Append(new StoreNumber(1))
            .Build(Id.NewId());

        var secondSource = sourceBuilder
            .Append(expectedDelta)
            .Build(Id.NewId());

        var versionTwoQuery = new EntityVersionQuery(new Version(2), new Version(2));

        await using var sourceRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ISourceRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        var firstSourceCommitted = await sourceRepository.Commit(firstSource);

        // ARRANGE ASSERTIONS

        firstSourceCommitted.ShouldBeTrue();

        // ACT

        var secondSourceCommitted = await sourceRepository.Commit(secondSource);

        var newDeltas = await sourceRepository.EnumerateDeltas(versionTwoQuery).ToArrayAsync();

        // ASSERT

        secondSourceCommitted.ShouldBeTrue();

        secondSource.Messages.Length.ShouldBe(1);

        secondSource.Messages[0].EntityPointer.Version.ShouldBe(new Version(2));

        newDeltas.Length.ShouldBe(1);

        newDeltas[0].ShouldBeEquivalentTo(expectedDelta);
    }

    private async Task
        Generic_GivenSourceAlreadyCommitted_WhenQueryingBySourceTimeStamp_ThenReturnExpectedObjects<TEntity>(
            SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        const ulong timeSpanInMinutes = 60UL;
        const ulong gteInMinutes = 20UL;
        const ulong lteInMinutes = 30UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var originTimeStamp = TimeStamp.UnixEpoch;

        var sources = new List<Source>();
        var expectedObjects = new ExpectedObjects();

        var sourceIds = GetSortedIds((int)timeSpanInMinutes);
        var entityIds = GetSortedIds((int)timeSpanInMinutes);

        TimeStamp? gte = null;
        TimeStamp? lte = null;

        for (var i = 1UL; i <= timeSpanInMinutes; i++)
        {
            var currentSourceId = sourceIds[i - 1];
            var currentEntityId = entityIds[i - 1];
            var currentSourceTimeStamp = new TimeStamp(originTimeStamp.Value.AddMinutes(i));

            var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

            var deltas = new object[] { new StoreNumber(i) };

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i is >= gteInMinutes and <= lteInMinutes, currentSourceId, currentEntityId,
                agentSignature, deltas, leases, tags);

            switch (i)
            {
                case lteInMinutes:
                    lte = currentSourceTimeStamp;
                    break;

                case gteInMinutes:
                    gte = currentSourceTimeStamp;
                    break;
            }

            var source = await BuildSource<TEntity>(serviceScope, currentSourceId, currentEntityId,
                new[] { i },
                currentSourceTimeStamp, agentSignature);

            sources.Add(source);
        }

        gte.ShouldNotBeNull();
        lte.ShouldNotBeNull();

        var query = new SourceTimeStampQuery(gte.Value, lte.Value);

        await PutSources(serviceScope, sources);
        await TestGetSourceIds(serviceScope, query as IMessageGroupQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as IMessageQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as IMessageGroupQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as IMessageQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetAgentSignatures(serviceScope, query, expectedObjects);
        await TestGetDeltas(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    private async Task
        Generic_GivenSourceAlreadyCommitted_WhenQueryingBySourceId_ThenReturnExpectedObjects<TEntity>(
            SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        const ulong numberOfSourceIds = 10UL;
        const ulong whichSourceId = 5UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var sources = new List<Source>();
        var expectedObjects = new ExpectedObjects();

        Id? sourceId = null;

        var sourceIds = GetSortedIds((int)numberOfSourceIds);
        var entityIds = GetSortedIds((int)numberOfSourceIds);

        var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        for (var i = 1UL; i <= numberOfSourceIds; i++)
        {
            var currentSourceId = sourceIds[i - 1];
            var currentEntityId = entityIds[i - 1];

            var deltas = new object[] { new StoreNumber(i) };

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i == whichSourceId, currentSourceId, currentEntityId, agentSignature,
                deltas,
                leases, tags);

            if (i == whichSourceId) sourceId = currentSourceId;

            var source = await BuildSource<TEntity>(serviceScope, currentSourceId, currentEntityId,
                new[] { i },
                agentSignatureOverride: agentSignature);

            sources.Add(source);
        }

        sourceId.ShouldNotBeNull();

        var query = new SourceIdQuery(sourceId.Value);

        await PutSources(serviceScope, sources);
        await TestGetSourceIds(serviceScope, query as IMessageGroupQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as IMessageQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as IMessageGroupQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as IMessageQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetAgentSignatures(serviceScope, query, expectedObjects);
        await TestGetDeltas(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    private async Task
        Generic_GivenSourceAlreadyCommitted_WhenQueryingByEntityId_ThenReturnExpectedObjects<TEntity>(
            SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        const ulong numberOfEntityIds = 10UL;
        const ulong whichEntityId = 5UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var sources = new List<Source>();
        var expectedObjects = new ExpectedObjects();

        Id? entityId = null;

        var sourceIds = GetSortedIds((int)numberOfEntityIds);
        var entityIds = GetSortedIds((int)numberOfEntityIds);

        var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        for (var i = 1UL; i <= numberOfEntityIds; i++)
        {
            var currentSourceId = sourceIds[i - 1];
            var currentEntityId = entityIds[i - 1];

            var deltas = new object[] { new StoreNumber(i) };

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i == whichEntityId, currentSourceId, currentEntityId, agentSignature, deltas,
                leases, tags);

            if (i == whichEntityId) entityId = currentEntityId;

            var source = await BuildSource<TEntity>(serviceScope, currentSourceId, currentEntityId,
                new[] { i },
                agentSignatureOverride: agentSignature);

            sources.Add(source);
        }

        entityId.ShouldNotBeNull();

        var query = new EntityQuery(entityId.Value);

        await PutSources(serviceScope, sources);
        await TestGetSourceIds(serviceScope, query as IMessageGroupQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as IMessageQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as IMessageGroupQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as IMessageQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetAgentSignatures(serviceScope, query, expectedObjects);
        await TestGetDeltas(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    private async Task
        Generic_GivenSourceAlreadyCommitted_WhenQueryingByEntityVersion_ThenReturnExpectedObjects<TEntity>(
            SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        const ulong numberOfVersions = 20;
        const ulong gte = 5UL;
        const ulong lte = 15UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var counts = new List<ulong>();
        var expectedObjects = new ExpectedObjects();

        for (var i = 1UL; i <= numberOfVersions; i++)
        {
            var delta = new StoreNumber(i);

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            counts.Add(i);

            expectedObjects.Add(i is >= gte and <= lte, default, default, default!, new[] { delta },
                leases, tags);
        }

        var source = await BuildSource<TEntity>(serviceScope, Id.NewId(), Id.NewId(), counts.ToArray());

        var sources = new List<Source> { source };

        var query = new EntityVersionQuery(new Version(gte), new Version(lte));

        await PutSources(serviceScope, sources);
        await TestGetDeltas(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    private async Task Generic_GivenSourceAlreadyCommitted_WhenQueryingByData_ThenReturnExpectedObjects<TOptions,
        TEntity>(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
        where TOptions : class
        where TEntity : IEntity<TEntity>
    {
        const ulong countTo = 20UL;
        const ulong gte = 5UL;
        const ulong lte = 15UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var sources = new List<Source>();
        var expectedObjects = new ExpectedObjects();

        var sourceIds = GetSortedIds((int)countTo);
        var entityIds = GetSortedIds((int)countTo);

        var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        var deltas = new object[] { new DoNothing() };

        for (var i = 1UL; i <= countTo; i++)
        {
            var currentSourceId = sourceIds[i - 1];
            var currentEntityId = entityIds[i - 1];

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i is >= gte and <= lte, currentSourceId, currentEntityId, agentSignature, deltas,
                leases, tags);

            var source = await BuildSource<TEntity>(serviceScope, currentSourceId, currentEntityId,
                new[] { i },
                agentSignatureOverride: agentSignature);

            sources.Add(source);
        }

        var options = serviceScope.ServiceProvider
            .GetRequiredService<IOptionsFactory<TOptions>>()
            .Create("Count");

        var query = new CountQuery(gte, lte, options);

        await PutSources(serviceScope, sources);
        await TestGetSourceIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }


    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenReadOnlyMode_WhenCommittingSource_ThenCannotWriteInReadOnlyModeExceptionIsLogged(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenNonUniqueSourceIds_WhenCommittingSources_ThenSecondPutReturnsFalse(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenNonUniqueVersions_WhenCommittingDeltas_ThenReturnFalse(SourcesAdder sourcesAdder,
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenVersionZero_WhenCommittingDeltas_ThenReturnTrue(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenNonUniqueVersions_WhenCommittingDeltas_ThenOptimisticConcurrencyExceptionIsLogged(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenNonUniqueTags_WhenCommittingTags_ThenReturnTrue(SourcesAdder sourcesAdder,
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenNonUniqueLeases_WhenCommittingLeases_ThenReturnFalse(SourcesAdder sourcesAdder,
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenDeltaCommitted_WhenGettingAnnotatedAgentSignature_ThenReturnAnnotatedAgentSignature(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenDeltaCommitted_WhenGettingAnnotatedDeltas_ThenReturnAnnotatedDelta(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenEntityCommitted_WhenGettingEntity_ThenReturnEntity(SourcesAdder sourcesAdder,
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenEntityCommittedWithTags_WhenRemovingAllTags_ThenFinalEntityHasNoTags(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenEntityCommittedWithLeases_WhenRemovingAllLeases_ThenFinalEntityHasNoLeases(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenSourceCreatesEntity_WhenQueryingForVersionOne_ThenReturnTheExpectedDelta(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenSourceAppendsEntityWithOneVersion_WhenQueryingForVersionTwo_ThenReturnExpectedDelta(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenSourceAlreadyCommitted_WhenQueryingBySourceTimeStamp_ThenReturnExpectedObjects(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenSourceAlreadyCommitted_WhenQueryingBySourceId_ThenReturnExpectedObjects(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenSourceAlreadyCommitted_WhenQueryingByEntityId_ThenReturnExpectedObjects(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenSourceAlreadyCommitted_WhenQueryingByEntityVersion_ThenReturnExpectedObjects(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddSourcesAndEntity))]
    public Task GivenSourceAlreadyCommitted_WhenQueryingByData_ThenReturnExpectedObjects(
        SourcesAdder sourcesAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { sourcesAdder.QueryOptionsType, entityAdder.EntityType },
            new object?[] { sourcesAdder, entityAdder }
        );
    }

    private class ExpectedObjects
    {
        public readonly List<object> FalseAgentSignatures = new();
        public readonly List<object> FalseDeltas = new();
        public readonly List<Id> FalseEntityIds = new();
        public readonly List<ILease> FalseLeases = new();
        public readonly List<Id> FalseSourceIds = new();
        public readonly List<ITag> FalseTags = new();

        public readonly List<object> TrueAgentSignatures = new();
        public readonly List<object> TrueDeltas = new();
        public readonly List<Id> TrueEntityIds = new();
        public readonly List<ILease> TrueLeases = new();
        public readonly List<Id> TrueSourceIds = new();
        public readonly List<ITag> TrueTags = new();

        public void Add
        (
            bool condition,
            Id sourceId,
            Id entityId,
            object agentSignature,
            IEnumerable<object> deltas,
            IEnumerable<ILease> leases,
            IEnumerable<ITag> tags
        )
        {
            if (condition)
            {
                TrueSourceIds.Add(sourceId);
                TrueEntityIds.Add(entityId);
                TrueAgentSignatures.Add(agentSignature);
                TrueDeltas.AddRange(deltas);
                TrueLeases.AddRange(leases);
                TrueTags.AddRange(tags);
            }
            else
            {
                FalseSourceIds.Add(sourceId);
                FalseEntityIds.Add(entityId);
                FalseAgentSignatures.Add(agentSignature);
                FalseDeltas.AddRange(deltas);
                FalseLeases.AddRange(leases);
                FalseTags.AddRange(tags);
            }
        }
    }
}