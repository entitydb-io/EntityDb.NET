using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.States.Attributes;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Sources.Agents;
using EntityDb.Common.Sources.Queries;
using EntityDb.Common.Sources.Queries.Modified;
using EntityDb.Common.Sources.Queries.Standard;
using EntityDb.Common.Tests.Implementations.Entities.Deltas;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Sources.Queries;
using EntityDb.Common.Tests.Implementations.States.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using System.Diagnostics.CodeAnalysis;
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
        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

        foreach (var source in sources)
        {
            var committed = await writeRepository.Commit(source);

            committed.ShouldBeTrue();
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

        await using var readOnlyRepository = await GetReadOnlySourceRepository(serviceScope, secondaryPreferred);

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

        // ACT

        var actualTrueResults =
            await getActualResults.Invoke(readOnlyRepository, bufferModifier).ToArrayAsync();
        var actualFalseResults =
            await getActualResults.Invoke(readOnlyRepository, negateModifier).ToArrayAsync();
        var reversedActualTrueResults =
            await getActualResults.Invoke(readOnlyRepository, reverseBufferModifier).ToArrayAsync();
        var reversedActualFalseResults =
            await getActualResults.Invoke(readOnlyRepository, reverseNegateModifier).ToArrayAsync();
        var actualSkipTakeResults =
            await getActualResults.Invoke(readOnlyRepository, bufferSubsetModifier).ToArrayAsync();

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
        ISourceDataDataQuery dataQuery,
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
            return sourceRepository.EnumerateSourceIds(dataQuery.Modify(modifiedQueryOptions));
        }
    }

    private static async Task TestGetSourceIds
    (
        IServiceScope serviceScope,
        IMessageDataDataQuery dataQuery,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository.EnumerateSourceIds(dataQuery.Modify(modifiedQueryOptions));
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
        ILeaseDataDataQuery dataDataQuery,
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
            return sourceRepository.EnumerateSourceIds(dataDataQuery.Modify(modifiedQueryOptions));
        }
    }

    private static async Task TestGetSourceIds
    (
        IServiceScope serviceScope,
        ITagDataDataQuery dataDataQuery,
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
            return sourceRepository.EnumerateSourceIds(dataDataQuery.Modify(modifiedQueryOptions));
        }
    }

    private static async Task TestGetStateIds
    (
        IServiceScope serviceScope,
        ISourceDataDataQuery dataQuery,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseStateIds
                    : expectedObjects.TrueStateIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository
                .EnumerateStatePointers(dataQuery.Modify(modifiedQueryOptions))
                .Select(pointer => pointer.Id);
        }
    }

    private static async Task TestGetStateIds
    (
        IServiceScope serviceScope,
        IMessageDataDataQuery dataQuery,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseStateIds
                    : expectedObjects.TrueStateIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository
                .EnumerateStatePointers(dataQuery.Modify(modifiedQueryOptions))
                .Select(pointer => pointer.Id);
        }
    }

    private static async Task TestGetStateIds
    (
        IServiceScope serviceScope,
        ILeaseDataDataQuery dataDataQuery,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseStateIds
                    : expectedObjects.TrueStateIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository
                .EnumerateStatePointers(dataDataQuery.Modify(modifiedQueryOptions))
                .Select(pointer => pointer.Id);
        }
    }

    private static async Task TestGetStateIds
    (
        IServiceScope serviceScope,
        ITagDataDataQuery dataDataQuery,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseStateIds
                    : expectedObjects.TrueStateIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository
                .EnumerateStatePointers(dataDataQuery.Modify(modifiedQueryOptions))
                .Select(pointer => pointer.Id);
        }
    }

    private static async Task TestGetAgentSignatures
    (
        IServiceScope serviceScope,
        ISourceDataDataQuery dataQuery,
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
            return sourceRepository.EnumerateAgentSignatures(dataQuery.Modify(modifiedQueryOptions));
        }
    }

    private static async Task TestGetDeltas
    (
        IServiceScope serviceScope,
        IMessageDataDataQuery dataQuery,
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
            return sourceRepository.EnumerateDeltas(dataQuery.Modify(modifiedQueryOptions));
        }
    }

    private static async Task TestGetLeases
    (
        IServiceScope serviceScope,
        ILeaseDataDataQuery dataDataQuery,
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
            return sourceRepository.EnumerateLeases(dataDataQuery.Modify(modifiedQueryOptions));
        }
    }

    private static async Task TestGetTags
    (
        IServiceScope serviceScope,
        ITagDataDataQuery dataDataQuery,
        ExpectedObjects expectedObjects
    )
    {
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);

        return;

        IAsyncEnumerable<ITag> GetActualResults(ISourceRepository sourceRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return sourceRepository.EnumerateTags(dataDataQuery.Modify(modifiedQueryOptions));
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
        TimeStamp? timeStamp = null,
        Id? stateId = null,
        object? agentSignature = null
    )
    {
        var nonNullableStateId = stateId ?? Id.NewId();

        return new Source
        {
            Id = id ?? Id.NewId(),
            TimeStamp = timeStamp ?? TimeStamp.UtcNow,
            AgentSignature = agentSignature ?? new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = versionNumbers
                .Select(versionNumber => new Message
                {
                    Id = Id.NewId(),
                    StatePointer = nonNullableStateId + new Version(versionNumber),
                    Delta = new StoreNumber(versionNumber),
                })
                .ToArray(),
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

    private async Task Generic_GivenSourceAlreadyCommitted_WhenQueryingByData_ThenReturnExpectedObjects<TOptions>(
        SourceRepositoryAdder sourceRepositoryAdder)
        where TOptions : class
    {
        const ulong countTo = 20UL;
        const ulong gte = 5UL;
        const ulong lte = 15UL;

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        var sources = new List<Source>();
        var expectedObjects = new ExpectedObjects();

        var sourceIds = GetSortedIds((int)countTo);
        var stateIds = GetSortedIds((int)countTo);

        var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        var deltas = new object[] { new DoNothing() };

        for (var i = 1UL; i <= countTo; i++)
        {
            var currentSourceId = sourceIds[i - 1];
            var currentStateId = stateIds[i - 1];

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i is >= gte and <= lte, currentSourceId, currentStateId, agentSignature, deltas,
                leases, tags);

            var source = new Source
            {
                Id = currentSourceId,
                TimeStamp = TimeStamp.UtcNow,
                AgentSignature = agentSignature,
                Messages = deltas
                    .Select(delta => new Message
                    {
                        Id = Id.NewId(),
                        StatePointer = currentStateId,
                        Delta = delta,
                        AddLeases = leases.ToArray<ILease>(),
                        AddTags = tags.ToArray<ITag>(),
                    })
                    .ToArray(),
            };

            sources.Add(source);
        }

        var options = serviceScope.ServiceProvider
            .GetRequiredService<IOptionsFactory<TOptions>>()
            .Create("Count");

        var query = new CountDataDataDataDataQuery(gte, lte, options);

        await PutSources(serviceScope, sources);
        await TestGetSourceIds(serviceScope, query as ILeaseDataDataQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ITagDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as ILeaseDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as ITagDataDataQuery, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenReadOnlyMode_WhenCommittingSource_ThenReadOnlyWriteExceptionIsLogged(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        var logs = new List<Log>();

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(GetMockedLoggerFactory(logs));
        });

        await using var readOnlyRepository = await GetReadOnlySourceRepository(serviceScope);

        var source = CreateSource(new[] { 1ul });

        // ACT

        var committed = await readOnlyRepository.Commit(source);

        // ASSERT

        committed.ShouldBeFalse();

        logs.Count(log => log.Exception is ReadOnlyWriteException).ShouldBe(1);
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenNonUniqueSourceIds_WhenCommittingSources_ThenSecondPutReturnsFalse(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

        var firstSource = CreateSource(new[] { 1ul });
        var secondSource = CreateSource(new[] { 1ul }, firstSource.Id);

        // ACT

        var firstSourceCommitted = await writeRepository.Commit(firstSource);
        var secondSourceCommitted = await writeRepository.Commit(secondSource);

        // ASSERT

        firstSourceCommitted.ShouldBeTrue();
        secondSourceCommitted.ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenNonUniqueVersions_WhenCommittingDeltas_ThenReturnFalse(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

        const int repeatCount = 2;

        var source = CreateSource(new[] { 1ul });

        source = source with
        {
            Messages = Enumerable
                .Repeat(source.Messages, repeatCount)
                .SelectMany(messages => messages)
                .ToArray(),
        };

        // ARRANGE ASSERTIONS

        repeatCount.ShouldBeGreaterThan(1);

        // ACT

        var committed = await writeRepository.Commit(source);

        // ASSERT

        committed.ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenVersionZero_WhenCommittingDeltas_ThenReturnTrue(SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

        var source = CreateSource(new[] { 0ul });

        // ACT

        var committed = await writeRepository.Commit(source);

        // ASSERT

        committed.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenNonUniqueVersions_WhenCommittingDeltas_ThenOptimisticConcurrencyExceptionIsLogged(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        var logs = new List<Log>();

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(GetMockedLoggerFactory(logs));
        });

        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

        var stateId = Id.NewId();

        var firstSource = CreateSource(new[] { 1ul }, stateId: stateId);
        var secondSource = CreateSource(new[] { 1ul }, stateId: stateId);

        // ACT

        var firstSourceCommitted = await writeRepository.Commit(firstSource);
        var secondSourceCommitted = await writeRepository.Commit(secondSource);

        // ASSERT

        firstSourceCommitted.ShouldBeTrue();
        secondSourceCommitted.ShouldBeFalse();

        logs.Count(log => log.Exception is OptimisticConcurrencyException).ShouldBe(1);
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenNonUniqueTags_WhenCommittingTags_ThenReturnTrue(SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

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
                    StatePointer = default,
                    Delta = new DoNothing(),
                    AddTags = new[] { tag, tag }.ToArray(),
                },
            },
        };

        // ACT

        var committed = await writeRepository.Commit(source);

        // ASSERT

        committed.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenNonUniqueLeases_WhenCommittingLeases_ThenReturnFalse(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

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
                    Id = default, StatePointer = default, Delta = new DoNothing(), AddLeases = new[] { lease, lease },
                },
            },
        };

        // ACT

        var committed = await writeRepository.Commit(source);

        // ASSERT

        committed.ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenDeltaCommitted_WhenGettingAnnotatedAgentSignature_ThenReturnAnnotatedAgentSignature(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

        var expectedSourceId = Id.NewId();
        var expectedStateId = Id.NewId();
        var expectedSourceTimeStamp = sourceRepositoryAdder.FixTimeStamp(TimeStamp.UtcNow);
        var expectedAgentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        var source = CreateSource
        (
            new[] { 1ul },
            expectedSourceId,
            expectedSourceTimeStamp,
            expectedStateId,
            expectedAgentSignature
        );

        var committed = await writeRepository.Commit(source);

        var query = new StateDataDataDataDataQuery(expectedStateId);

        // ARRANGE ASSERTIONS

        committed.ShouldBeTrue();

        // ACT

        var annotatedAgentSignatures = await writeRepository
            .EnumerateAnnotatedAgentSignatures(query)
            .ToArrayAsync();

        // ASSERT

        annotatedAgentSignatures.Length.ShouldBe(1);

        annotatedAgentSignatures[0].SourceId.ShouldBe(expectedSourceId);
        annotatedAgentSignatures[0].SourceTimeStamp.ShouldBe(expectedSourceTimeStamp);
        annotatedAgentSignatures[0].StatePointers.Length.ShouldBe(1);
        annotatedAgentSignatures[0].StatePointers[0].Id.ShouldBe(expectedStateId);
        annotatedAgentSignatures[0].Data.ShouldBeEquivalentTo(expectedAgentSignature);
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenDeltaCommitted_WhenGettingAnnotatedDeltas_ThenReturnAnnotatedDelta(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        ulong[] numbers = { 1, 2, 3, 4, 5 };

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

        var expectedSourceId = Id.NewId();
        var expectedStateId = Id.NewId();
        var expectedSourceTimeStamp = sourceRepositoryAdder.FixTimeStamp(TimeStamp.UtcNow);

        var source = CreateSource
        (
            numbers,
            expectedSourceId,
            expectedSourceTimeStamp,
            expectedStateId
        );

        var committed = await writeRepository.Commit(source);

        var query = new GetDeltasDataQuery(expectedStateId, default);

        // ARRANGE ASSERTIONS

        committed.ShouldBeTrue();

        // ACT

        var annotatedDeltas = await writeRepository.EnumerateAnnotatedDeltas(query).ToArrayAsync();

        // ASSERT

        annotatedDeltas.Length.ShouldBe(numbers.Length);

        foreach (var number in numbers)
        {
            var annotatedDelta = annotatedDeltas[Convert.ToInt32(number) - 1];

            annotatedDelta.SourceId.ShouldBe(expectedSourceId);
            annotatedDelta.SourceTimeStamp.ShouldBe(expectedSourceTimeStamp);
            annotatedDelta.StatePointer.Id.ShouldBe(expectedStateId);
            annotatedDelta.StatePointer.Version.ShouldBe(new Version(number));

            annotatedDelta.Data
                .ShouldBeAssignableTo<StoreNumber>()
                .ShouldNotBeNull()
                .Number
                .ShouldBe(number);
        }
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenMessageWithTagsCommitted_WhenRemovingAllTags_ThenSourceHasNoTags(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

        var stateId = Id.NewId();

        var tag = TagSeeder.Create();
        var tags = new[] { tag };

        var initialSource = new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = new[]
            {
                new Message
                {
                    Id = Id.NewId(), Delta = new DoNothing(), StatePointer = stateId, AddTags = tags,
                },
            },
        };

        var initialSourceCommitted = await writeRepository.Commit(initialSource);

        var finalSource = new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = new[]
            {
                new Message
                {
                    Id = Id.NewId(), Delta = new DoNothing(), StatePointer = stateId, DeleteTags = tags,
                },
            },
        };

        var deleteTagsQuery = new DeleteTagsDataQuery(stateId, tags);

        // ARRANGE ASSERTIONS

        initialSourceCommitted.ShouldBeTrue();

        // ACT

        var initialTags = await writeRepository
            .EnumerateTags(deleteTagsQuery)
            .ToArrayAsync();

        var finalSourceCommitted = await writeRepository.Commit(finalSource);

        var finalTags = await writeRepository
            .EnumerateTags(deleteTagsQuery)
            .ToArrayAsync();

        // ASSERT

        initialTags.Length.ShouldBe(1);
        initialTags[0].ShouldBeEquivalentTo(tag);
        finalSourceCommitted.ShouldBeTrue();
        finalTags.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenMessageWithLeasesCommitted_WhenRemovingAllLeases_ThenSourceHasNoLeases(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

        var lease = LeaseSeeder.Create();
        var leases = new[] { lease };

        var initialSource = new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = new[]
            {
                new Message
                {
                    Id = Id.NewId(), Delta = new DoNothing(), StatePointer = default, AddLeases = leases,
                },
            },
        };

        var initialSourceCommitted = await writeRepository.Commit(initialSource);

        var finalSource = new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = new[]
            {
                new Message
                {
                    Id = Id.NewId(), Delta = new DoNothing(), StatePointer = default, DeleteLeases = leases,
                },
            },
        };

        var leaseQuery = new DeleteLeasesDataQuery(leases);

        // ARRANGE ASSERTIONS

        initialSourceCommitted.ShouldBeTrue();

        // ACT

        var initialLeases = await writeRepository
            .EnumerateLeases(leaseQuery)
            .ToArrayAsync();

        var finalSourceCommitted = await writeRepository.Commit(finalSource);

        var finalLeases = await writeRepository
            .EnumerateLeases(leaseQuery)
            .ToArrayAsync();

        // ASSERT

        initialLeases.Length.ShouldBe(1);
        finalSourceCommitted.ShouldBeTrue();
        finalLeases.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenMessageCommitted_WhenQueryingForVersionOne_ThenReturnTheExpectedDelta(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

        var expectedDelta = new StoreNumber(1);

        var source = CreateSource(new[] { 1ul });

        var versionOneQuery = new StateVersionDataDataDataDataQuery(new Version(1), new Version(1));

        // ACT

        var committed = await writeRepository.Commit(source);

        var newDeltas = await writeRepository.EnumerateDeltas(versionOneQuery).ToArrayAsync();

        // ASSERT

        committed.ShouldBeTrue();
        newDeltas.Length.ShouldBe(1);
        newDeltas[0].ShouldBeEquivalentTo(expectedDelta);
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenTwoMessagesCommitted_WhenQueryingForVersionTwo_ThenReturnExpectedDelta(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        await using var writeRepository = await GetWriteSourceRepository(serviceScope);

        var expectedDelta = new StoreNumber(2);

        var stateId = Id.NewId();
        var firstSource = CreateSource(new[] { 1ul }, stateId: stateId);
        var secondSource = CreateSource(new[] { 2ul }, stateId: stateId);

        var versionTwoQuery = new StateVersionDataDataDataDataQuery(new Version(2), new Version(2));

        // ACT

        var firstSourceCommitted = await writeRepository.Commit(firstSource);

        var secondSourceCommitted = await writeRepository.Commit(secondSource);

        var newDeltas = await writeRepository.EnumerateDeltas(versionTwoQuery).ToArrayAsync();

        // ASSERT

        firstSourceCommitted.ShouldBeTrue();
        secondSourceCommitted.ShouldBeTrue();
        newDeltas.Length.ShouldBe(1);
        newDeltas[0].ShouldBeEquivalentTo(expectedDelta);
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenSourceAlreadyCommitted_WhenQueryingBySourceTimeStamp_ThenReturnExpectedObjects(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        const ulong timeSpanInMinutes = 60UL;
        const ulong gteInMinutes = 20UL;
        const ulong lteInMinutes = 30UL;

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        var originTimeStamp = TimeStamp.UnixEpoch;

        var sources = new List<Source>();
        var expectedObjects = new ExpectedObjects();

        var sourceIds = GetSortedIds((int)timeSpanInMinutes);
        var stateIds = GetSortedIds((int)timeSpanInMinutes);

        TimeStamp? gte = null;
        TimeStamp? lte = null;

        for (var i = 1UL; i <= timeSpanInMinutes; i++)
        {
            var currentSourceId = sourceIds[i - 1];
            var currentStateId = stateIds[i - 1];
            var currentSourceTimeStamp = new TimeStamp(originTimeStamp.Value.AddMinutes(i));

            var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

            var deltas = new object[] { new StoreNumber(i) };
            var leases = new[] { new CountLease(i) };
            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i is >= gteInMinutes and <= lteInMinutes, currentSourceId, currentStateId,
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
                    .Select(delta => new Message
                    {
                        Id = Id.NewId(),
                        StatePointer = currentStateId,
                        Delta = delta,
                        AddLeases = leases.ToArray<ILease>(),
                        AddTags = tags.ToArray<ITag>(),
                    })
                    .ToArray(),
            };

            sources.Add(source);
        }

        gte.ShouldNotBeNull();
        lte.ShouldNotBeNull();

        var query = new SourceTimeStampDataDataDataDataQuery(gte.Value, lte.Value);

        await PutSources(serviceScope, sources);
        await TestGetSourceIds(serviceScope, query as ISourceDataDataQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as IMessageDataDataQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ILeaseDataDataQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ITagDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as ISourceDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as IMessageDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as ILeaseDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as ITagDataDataQuery, expectedObjects);
        await TestGetAgentSignatures(serviceScope, query, expectedObjects);
        await TestGetDeltas(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenSourceAlreadyCommitted_WhenQueryingBySourceId_ThenReturnExpectedObjects(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        const ulong numberOfSourceIds = 10UL;
        const ulong whichSourceId = 5UL;

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        var sources = new List<Source>();
        var expectedObjects = new ExpectedObjects();

        Id? sourceId = null;

        var sourceIds = GetSortedIds((int)numberOfSourceIds);
        var stateIds = GetSortedIds((int)numberOfSourceIds);

        var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        for (var i = 1UL; i <= numberOfSourceIds; i++)
        {
            var currentSourceId = sourceIds[i - 1];
            var currentStateId = stateIds[i - 1];

            var deltas = new object[] { new StoreNumber(i) };
            var leases = new[] { new CountLease(i) };
            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i == whichSourceId, currentSourceId, currentStateId, agentSignature,
                deltas,
                leases, tags);

            if (i == whichSourceId)
            {
                sourceId = currentSourceId;
            }

            var source = new Source
            {
                Id = currentSourceId,
                TimeStamp = TimeStamp.UtcNow,
                AgentSignature = agentSignature,
                Messages = deltas
                    .Select(delta => new Message
                    {
                        Id = Id.NewId(),
                        StatePointer = currentStateId,
                        Delta = delta,
                        AddLeases = leases.ToArray<ILease>(),
                        AddTags = tags.ToArray<ITag>(),
                    })
                    .ToArray(),
            };

            sources.Add(source);
        }

        sourceId.ShouldNotBeNull();

        var query = new SourceIdDataDataDataDataQuery(sourceId.Value);

        await PutSources(serviceScope, sources);
        await TestGetSourceIds(serviceScope, query as ISourceDataDataQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as IMessageDataDataQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ILeaseDataDataQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ITagDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as ISourceDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as IMessageDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as ILeaseDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as ITagDataDataQuery, expectedObjects);
        await TestGetAgentSignatures(serviceScope, query, expectedObjects);
        await TestGetDeltas(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenSourceAlreadyCommitted_WhenQueryingByStateId_ThenReturnExpectedObjects(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        const ulong numberOfStateIds = 10UL;
        const ulong whichStateId = 5UL;

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
        });

        var sources = new List<Source>();
        var expectedObjects = new ExpectedObjects();

        Id? stateId = null;

        var sourceIds = GetSortedIds((int)numberOfStateIds);
        var stateIds = GetSortedIds((int)numberOfStateIds);

        var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        for (var i = 1UL; i <= numberOfStateIds; i++)
        {
            var currentSourceId = sourceIds[i - 1];
            var currentStateId = stateIds[i - 1];

            var deltas = new object[] { new StoreNumber(i) };

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i == whichStateId, currentSourceId, currentStateId, agentSignature, deltas,
                leases, tags);

            if (i == whichStateId)
            {
                stateId = currentStateId;
            }

            var source = new Source
            {
                Id = currentSourceId,
                TimeStamp = TimeStamp.UtcNow,
                AgentSignature = agentSignature,
                Messages = deltas
                    .Select(delta => new Message
                    {
                        Id = Id.NewId(),
                        StatePointer = currentStateId,
                        Delta = delta,
                        AddLeases = leases.ToArray<ILease>(),
                        AddTags = tags.ToArray<ITag>(),
                    })
                    .ToArray(),
            };

            sources.Add(source);
        }

        stateId.ShouldNotBeNull();

        var query = new StateDataDataDataDataQuery(stateId.Value);

        await PutSources(serviceScope, sources);
        await TestGetSourceIds(serviceScope, query as ISourceDataDataQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as IMessageDataDataQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ILeaseDataDataQuery, expectedObjects);
        await TestGetSourceIds(serviceScope, query as ITagDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as ISourceDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as IMessageDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as ILeaseDataDataQuery, expectedObjects);
        await TestGetStateIds(serviceScope, query as ITagDataDataQuery, expectedObjects);
        await TestGetAgentSignatures(serviceScope, query, expectedObjects);
        await TestGetDeltas(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenSourceAlreadyCommitted_WhenQueryingByStateVersion_ThenReturnExpectedObjects(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        const ulong numberOfVersions = 20;
        const ulong gte = 5UL;
        const ulong lte = 15UL;

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);
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
                StatePointer = default,
                Delta = delta,
                AddLeases = leases.ToArray<ILease>(),
                AddTags = tags.ToArray<ITag>(),
            });
        }

        var source = new Source
        {
            Id = Id.NewId(),
            TimeStamp = TimeStamp.UtcNow,
            AgentSignature = new UnknownAgentSignature(new Dictionary<string, string>()),
            Messages = messages.ToArray(),
        };

        var sources = new List<Source> { source };

        var query = new StateVersionDataDataDataDataQuery(new Version(gte), new Version(lte));

        await PutSources(serviceScope, sources);
        await TestGetDeltas(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public Task GivenSourceAlreadyCommitted_WhenQueryingByData_ThenReturnExpectedObjects(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        return RunGenericTestAsync
        (
            new[] { sourceRepositoryAdder.QueryOptionsType },
            new object?[] { sourceRepositoryAdder }
        );
    }

    private class ExpectedObjects
    {
        public readonly List<object> FalseAgentSignatures = new();
        public readonly List<object> FalseDeltas = new();
        public readonly List<ILease> FalseLeases = new();
        public readonly List<Id> FalseSourceIds = new();
        public readonly List<Id> FalseStateIds = new();
        public readonly List<ITag> FalseTags = new();

        public readonly List<object> TrueAgentSignatures = new();
        public readonly List<object> TrueDeltas = new();
        public readonly List<ILease> TrueLeases = new();
        public readonly List<Id> TrueSourceIds = new();
        public readonly List<Id> TrueStateIds = new();
        public readonly List<ITag> TrueTags = new();

        public void Add
        (
            bool condition,
            Id sourceId,
            Id stateId,
            object agentSignature,
            IEnumerable<object> deltas,
            IEnumerable<ILease> leases,
            IEnumerable<ITag> tags
        )
        {
            if (condition)
            {
                TrueSourceIds.Add(sourceId);
                TrueStateIds.Add(stateId);
                TrueAgentSignatures.Add(agentSignature);
                TrueDeltas.AddRange(deltas);
                TrueLeases.AddRange(leases);
                TrueTags.AddRange(tags);
            }
            else
            {
                FalseSourceIds.Add(sourceId);
                FalseStateIds.Add(stateId);
                FalseAgentSignatures.Add(agentSignature);
                FalseDeltas.AddRange(deltas);
                FalseLeases.AddRange(leases);
                FalseTags.AddRange(tags);
            }
        }
    }
}
