using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Sources.Agents;
using EntityDb.Common.Sources.Attributes;
using EntityDb.Common.Sources.Queries;
using EntityDb.Common.Sources.Queries.Modified;
using EntityDb.Common.Sources.Queries.Standard;
using EntityDb.Common.Tests.Implementations.Deltas;
using EntityDb.Common.Tests.Implementations.Leases;
using EntityDb.Common.Tests.Implementations.Queries;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Tags;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);

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

    private static Source CreateSource
    (
        IEnumerable<ulong> versionNumbers,
        Id? id = null,
        Id? entityId = null,
        TimeStamp? timeStamp = null,
        object? agentSignature = null,
        IDeltaSeeder? deltaSeeder = null
    )
    {
        var nonNullableEntityId = entityId ?? Id.NewId();

        deltaSeeder ??= new StoreNumberSeeder();
        
        return new Source
        {
            Id = id ?? Id.NewId(),
            TimeStamp = timeStamp ?? TimeStamp.UtcNow,
            AgentSignature = agentSignature ?? new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = versionNumbers
                .Select(versionNumber =>new Message
                {
                    Id = Id.NewId(),
                    EntityPointer = nonNullableEntityId + new Version(versionNumber),
                    Delta = deltaSeeder.Create(versionNumber),
                })
                .ToImmutableArray(),
        };
    }

    private static Id[] GetSortedIds(int numberOfIds)
    {
        return Enumerable
            .Range(1, numberOfIds)
            .Select(_ => Id.NewId())
            .OrderBy(id => id.Value)
            .ToArray();
    }

    private async Task Generic_GivenSourceAlreadyCommitted_WhenQueryingByData_ThenReturnExpectedObjects<TOptions>(SourcesAdder sourcesAdder)
        where TOptions : class
    {
        const ulong countTo = 20UL;
        const ulong gte = 5UL;
        const ulong lte = 15UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
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

            var source = new Source
            {
                Id = currentSourceId,
                TimeStamp = TimeStamp.UtcNow,
                AgentSignature = agentSignature,
                Messages = deltas
                    .Select(delta =>new Message
                    {
                        Id = Id.NewId(),
                        EntityPointer = currentEntityId,
                        Delta = delta,
                        AddLeases = leases.ToImmutableArray<ILease>(),
                        AddTags = tags.ToImmutableArray<ITag>(),
                    })
                    .ToImmutableArray(),
            };

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
    [MemberData(nameof(AddSource))]
    public async Task GivenReadOnlyMode_WhenCommittingSource_ThenCannotWriteInReadOnlyModeExceptionIsLogged(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        //var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<CannotWriteInReadOnlyModeException>();

        var logs = new List<Log>();
        
        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(GetMockedLoggerFactory(logs));
        });

        await using var sourceRepository = await GetReadOnlySourceRepository(serviceScope);

        var source = CreateSource(new[] { 1ul });

        // ACT

        var committed = await sourceRepository.Commit(source);

        // ASSERT

        committed.ShouldBeFalse();

        logs.Count(log => log.Exception is not null).ShouldBe(1);

        //loggerVerifier.Invoke(Times.Once());
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenNonUniqueSourceIds_WhenCommittingSources_ThenSecondPutReturnsFalse(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
        });
        
        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);

        var firstSource = CreateSource(new[] { 1ul });
        var secondSource = CreateSource(new[] { 1ul }, id: firstSource.Id);

        // ACT

        var firstSourceCommitted = await sourceRepository.Commit(firstSource);
        var secondSourceCommitted = await sourceRepository.Commit(secondSource);

        // ASSERT

        firstSourceCommitted.ShouldBeTrue();
        secondSourceCommitted.ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenNonUniqueVersions_WhenCommittingDeltas_ThenReturnFalse(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        const int repeatCount = 2;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);

        var source = CreateSource(new[] { 1ul });

        source = source with
        {
            Messages = Enumerable
                .Repeat(source.Messages, repeatCount)
                .SelectMany(messages => messages)
                .ToImmutableArray(),
        };

        // ARRANGE ASSERTIONS

        repeatCount.ShouldBeGreaterThan(1);

        // ACT

        var committed = await sourceRepository.Commit(source);

        // ASSERT

        committed.ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenVersionZero_WhenCommittingDeltas_ThenReturnTrue(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);

        var source = CreateSource(new[] { 0ul });

        // ACT

        var sourceCommitted = await sourceRepository.Commit(source);

        // ASSERT

        sourceCommitted.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenNonUniqueVersions_WhenCommittingDeltas_ThenOptimisticConcurrencyExceptionIsLogged(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        //var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<OptimisticConcurrencyException>();

        var logs = new List<Log>();
        
        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(GetMockedLoggerFactory(logs));
        });

        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);
        
        var entityId = Id.NewId();

        var firstSource = CreateSource(new[] { 1ul }, entityId: entityId);
        var secondSource = CreateSource(new[] { 1ul }, entityId: entityId);

        // ACT

        var firstSourceCommitted = await sourceRepository.Commit(firstSource);
        var secondSourceCommitted = await sourceRepository.Commit(secondSource);

        // ASSERT

        firstSourceCommitted.ShouldBeTrue();
        secondSourceCommitted.ShouldBeFalse();

        logs.Count(log => log.Exception is not null).ShouldBe(1);

        //loggerVerifier.Invoke(Times.Once());
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenNonUniqueTags_WhenCommittingTags_ThenReturnTrue(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);
        
        var tag = TagSeeder.Create();

        var source = new Source
        {
            Id = default,
            TimeStamp = default,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = new[]
            {
                new Message
                {
                    Id = default,
                    EntityPointer = default,
                    Delta = new DoNothing(),
                    AddTags = new[] { tag, tag }.ToImmutableArray(),
                },
            }.ToImmutableArray(),
        };

        // ACT

        var committed = await sourceRepository.Commit(source);

        // ASSERT

        committed.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenNonUniqueLeases_WhenCommittingLeases_ThenReturnFalse(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
        });
        
        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);

        var lease = LeaseSeeder.Create();

        var source = new Source
        {
            Id = default,
            TimeStamp = default,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = new[]
            {
                new Message
                {
                    Id = default,
                    EntityPointer = default,
                    Delta = new DoNothing(),
                    AddLeases = new[] { lease, lease }.ToImmutableArray(),
                },
            }.ToImmutableArray(),
        };

        // ACT

        var committed = await sourceRepository.Commit(source);

        // ASSERT

        committed.ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenDeltaCommitted_WhenGettingAnnotatedAgentSignature_ThenReturnAnnotatedAgentSignature(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
        });
        
        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);

        var expectedSourceId = Id.NewId();
        var expectedEntityId = Id.NewId();
        var expectedSourceTimeStamp = sourcesAdder.FixTimeStamp(TimeStamp.UtcNow);
        var expectedAgentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        var source = CreateSource
        (
            new[] { 1ul },
            id: expectedSourceId,
            timeStamp: expectedSourceTimeStamp,
            entityId: expectedEntityId,
            agentSignature: expectedAgentSignature
        );
        
        var committed = await sourceRepository.Commit(source);

        var query = new EntityQuery(expectedEntityId);

        // ARRANGE ASSERTIONS

        committed.ShouldBeTrue();

        // ACT

        var annotatedAgentSignatures = await sourceRepository
            .EnumerateAnnotatedAgentSignatures(query)
            .ToArrayAsync();

        // ASSERT

        annotatedAgentSignatures.Length.ShouldBe(1);

        annotatedAgentSignatures[0].SourceId.ShouldBe(expectedSourceId);
        annotatedAgentSignatures[0].SourceTimeStamp.ShouldBe(expectedSourceTimeStamp);
        annotatedAgentSignatures[0].EntityPointers.Length.ShouldBe(1);
        annotatedAgentSignatures[0].EntityPointers[0].Id.ShouldBe(expectedEntityId);
        annotatedAgentSignatures[0].Data.ShouldBeEquivalentTo(expectedAgentSignature);
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenDeltaCommitted_WhenGettingAnnotatedDeltas_ThenReturnAnnotatedDelta(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        ulong[] numbers = { 1, 2, 3, 4, 5 };
        
        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
        });
        
        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);

        var expectedSourceId = Id.NewId();
        var expectedEntityId = Id.NewId();
        var expectedSourceTimeStamp = sourcesAdder.FixTimeStamp(TimeStamp.UtcNow);

        var source = CreateSource
        (
            numbers,
            id: expectedSourceId,
            timeStamp: expectedSourceTimeStamp,
            entityId: expectedEntityId
        );

        var committed = await sourceRepository.Commit(source);

        var query = new GetDeltasQuery(expectedEntityId, default);

        // ARRANGE ASSERTIONS

        committed.ShouldBeTrue();

        // ACT

        var annotatedDeltas = await sourceRepository.EnumerateAnnotatedDeltas(query).ToArrayAsync();

        // ASSERT

        annotatedDeltas.Length.ShouldBe(numbers.Length);

        foreach (var number in numbers)
        {
            var annotatedDelta = annotatedDeltas[Convert.ToInt32(number) - 1];
            
            annotatedDelta.SourceId.ShouldBe(expectedSourceId);
            annotatedDelta.SourceTimeStamp.ShouldBe(expectedSourceTimeStamp);
            annotatedDelta.EntityPointer.Id.ShouldBe(expectedEntityId);
            annotatedDelta.EntityPointer.Version.ShouldBe(new Version(number));

            annotatedDelta.Data
                .ShouldBeAssignableTo<StoreNumber>()
                .ShouldNotBeNull()
                .Number
                .ShouldBe(number);
        }
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenEntityCommittedWithTags_WhenRemovingAllTags_ThenFinalEntityHasNoTags(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);
        
        var entityId = Id.NewId();

        var tag = new Tag("Foo", "Bar");
        var tags = new[] { tag }.ToImmutableArray<ITag>();

        var initialSource = new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = new[]
            {
                new Message
                {
                    Id = Id.NewId(),
                    Delta = new DoNothing(),
                    EntityPointer = entityId,
                    AddTags = tags,
                },
            }.ToImmutableArray(),
        };

        var initialSourceCommitted = await sourceRepository.Commit(initialSource);
        
        var finalSource = new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = new[]
            {
                new Message
                {
                    Id = Id.NewId(),
                    Delta = new DoNothing(),
                    EntityPointer = entityId,
                    DeleteTags = tags,
                },
            }.ToImmutableArray(),
        };

        var tagQuery = new DeleteTagsQuery(entityId, tags);

        // ARRANGE ASSERTIONS

        initialSourceCommitted.ShouldBeTrue();

        // ACT

        var initialTags = await sourceRepository
            .EnumerateTags(tagQuery)
            .ToArrayAsync();

        var finalSourceCommitted = await sourceRepository.Commit(finalSource);

        var finalTags = await sourceRepository
            .EnumerateTags(tagQuery)
            .ToArrayAsync();

        // ASSERT

        initialTags.Length.ShouldBe(1);
        initialTags[0].ShouldBeEquivalentTo(tag);
        finalSourceCommitted.ShouldBeTrue();
        finalTags.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenEntityCommittedWithLeases_WhenRemovingAllLeases_ThenFinalEntityHasNoLeases(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);
        
        var lease = new Lease("Foo", "Bar", "Baz");
        var leases = new[] { lease }.ToImmutableArray<ILease>();

        var initialSource = new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = new[]
            {
                new Message
                {
                    Id = Id.NewId(),
                    Delta = new DoNothing(),
                    EntityPointer = default,
                    AddLeases = leases,
                }
            }.ToImmutableArray(),
        };
        
        var initialSourceCommitted = await sourceRepository.Commit(initialSource);
        
        var finalSource = new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = new[]
            {
                new Message
                {
                    Id = Id.NewId(),
                    Delta = new DoNothing(),
                    EntityPointer = default,
                    DeleteLeases = leases,
                },
            }.ToImmutableArray(),
        };

        var leaseQuery = new DeleteLeasesQuery(leases);

        // ARRANGE ASSERTIONS

        initialSourceCommitted.ShouldBeTrue();

        // ACT

        var initialLeases = await sourceRepository
            .EnumerateLeases(leaseQuery)
            .ToArrayAsync();
        
        var finalSourceCommitted = await sourceRepository.Commit(finalSource);

        var finalLeases = await sourceRepository
            .EnumerateLeases(leaseQuery)
            .ToArrayAsync();

        // ASSERT

        initialLeases.Length.ShouldBe(1);
        finalSourceCommitted.ShouldBeTrue();
        finalLeases.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenSourceCreatesEntity_WhenQueryingForVersionOne_ThenReturnTheExpectedDelta(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);

        var expectedDelta = new StoreNumber(1);

        var source = CreateSource(new[] { 1ul });

        var versionOneQuery = new EntityVersionQuery(new Version(1), new Version(1));

        // ACT

        var sourceCommitted = await sourceRepository.Commit(source);

        var newDeltas = await sourceRepository.EnumerateDeltas(versionOneQuery).ToArrayAsync();

        // ASSERT

        sourceCommitted.ShouldBeTrue();
        newDeltas.Length.ShouldBe(1);
        newDeltas[0].ShouldBeEquivalentTo(expectedDelta);
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenSourceAppendsEntityWithOneVersion_WhenQueryingForVersionTwo_ThenReturnExpectedDelta(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
        });
        
        await using var sourceRepository = await GetWriteSourceRepository(serviceScope);
        
        var expectedDelta = new StoreNumber(2);

        var entityId = Id.NewId();
        var firstSource = CreateSource(new[] { 1ul }, entityId: entityId);
        var secondSource = CreateSource(new[] { 2ul }, entityId: entityId);

        var versionTwoQuery = new EntityVersionQuery(new Version(2), new Version(2));

        // ACT

        var firstSourceCommitted = await sourceRepository.Commit(firstSource);

        var secondSourceCommitted = await sourceRepository.Commit(secondSource);

        var newDeltas = await sourceRepository.EnumerateDeltas(versionTwoQuery).ToArrayAsync();

        // ASSERT

        firstSourceCommitted.ShouldBeTrue();
        secondSourceCommitted.ShouldBeTrue();
        newDeltas.Length.ShouldBe(1);
        newDeltas[0].ShouldBeEquivalentTo(expectedDelta);
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenSourceAlreadyCommitted_WhenQueryingBySourceTimeStamp_ThenReturnExpectedObjects(SourcesAdder sourcesAdder)
    {
        const ulong timeSpanInMinutes = 60UL;
        const ulong gteInMinutes = 20UL;
        const ulong lteInMinutes = 30UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
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

            var source = new Source
            {
                Id = currentSourceId,
                TimeStamp = currentSourceTimeStamp,
                AgentSignature = agentSignature,
                Messages = deltas
                    .Select(delta =>new Message
                    {
                        Id = Id.NewId(),
                        EntityPointer = currentEntityId,
                        Delta = delta,
                        AddLeases = leases.ToImmutableArray<ILease>(),
                        AddTags = tags.ToImmutableArray<ITag>(),
                    })
                    .ToImmutableArray(),
            };

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

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenSourceAlreadyCommitted_WhenQueryingBySourceId_ThenReturnExpectedObjects(
        SourcesAdder sourcesAdder)
    {
        const ulong numberOfSourceIds = 10UL;
        const ulong whichSourceId = 5UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
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

            var source = new Source
            {
                Id = currentSourceId,
                TimeStamp = TimeStamp.UtcNow,
                AgentSignature = agentSignature,
                Messages = deltas
                    .Select(delta =>new Message
                    {
                        Id = Id.NewId(),
                        EntityPointer = currentEntityId,
                        Delta = delta,
                        AddLeases = leases.ToImmutableArray<ILease>(),
                        AddTags = tags.ToImmutableArray<ITag>(),
                    })
                    .ToImmutableArray(),
            };

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

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenSourceAlreadyCommitted_WhenQueryingByEntityId_ThenReturnExpectedObjects(SourcesAdder sourcesAdder)
    {
        const ulong numberOfEntityIds = 10UL;
        const ulong whichEntityId = 5UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
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
            
            var source = new Source
            {
                Id = currentSourceId,
                TimeStamp = TimeStamp.UtcNow,
                AgentSignature = agentSignature,
                Messages = deltas
                    .Select(delta =>new Message
                    {
                        Id = Id.NewId(),
                        EntityPointer = currentEntityId,
                        Delta = delta,
                        AddLeases = leases.ToImmutableArray<ILease>(),
                        AddTags = tags.ToImmutableArray<ITag>(),
                    })
                    .ToImmutableArray(),
            };

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

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenSourceAlreadyCommitted_WhenQueryingByEntityVersion_ThenReturnExpectedObjects(SourcesAdder sourcesAdder)
    {
        const ulong numberOfVersions = 20;
        const ulong gte = 5UL;
        const ulong lte = 15UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);
        });

        var counts = new List<ulong>();
        var expectedObjects = new ExpectedObjects();

        var messages = new List<Message>();

        for (var i = 1UL; i <= numberOfVersions; i++)
        {
            var delta = new StoreNumber(i);
            var leases = new[] { new CountLease(i) };
            var tags = new[] { new CountTag(i) };

            counts.Add(i);

            expectedObjects.Add(i is >= gte and <= lte, default, default, default!, new[] { delta },
                leases, tags);
            
            messages.Add(new Message
            {
                Id = Id.NewId(),
                EntityPointer = default,
                Delta = delta,
                AddLeases = leases.ToImmutableArray<ILease>(),
                AddTags = tags.ToImmutableArray<ITag>(),
            });
        }
        
        var source = new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = messages.ToImmutableArray(),
        };
        
        var sources = new List<Source> { source };

        var query = new EntityVersionQuery(new Version(gte), new Version(lte));

        await PutSources(serviceScope, sources);
        await TestGetDeltas(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public Task GivenSourceAlreadyCommitted_WhenQueryingByData_ThenReturnExpectedObjects(SourcesAdder sourcesAdder)
    {
        return RunGenericTestAsync
        (
            new[] { sourcesAdder.QueryOptionsType },
            new object?[] { sourcesAdder }
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