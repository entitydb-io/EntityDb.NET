using EntityDb.Abstractions;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.States;
using EntityDb.Common.Extensions;
using EntityDb.Common.Polyfills;
using EntityDb.Common.Sources.Attributes;
using EntityDb.Common.Sources.Queries.Standard;
using EntityDb.Common.Tests.Implementations.Entities.Deltas;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Streams;

[Collection(nameof(DatabaseContainerCollection))]
public sealed class StreamTests : TestsBase<Startup>
{
    public StreamTests(IServiceProvider serviceProvider, DatabaseContainerFixture databaseContainerFixture)
        : base(serviceProvider, databaseContainerFixture)
    {
    }

    [Fact]
    public async Task GivenNewStreamMock_WhenStagingNewMessageKey_ThenCommittedSourceIsCorrect()
    {
        // ARRANGE

        var committedSources = new List<Source>();

        var sequenceMock = new MockSequence();
        var sourceRepositoryMock = new Mock<ISourceRepository>(MockBehavior.Strict);

        // First query checks if stream key lease already exists
        sourceRepositoryMock
            .InSequence(sequenceMock)
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ILeaseDataQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<StatePointer>());

        // Second query checks if message key lease already exists
        sourceRepositoryMock
            .InSequence(sequenceMock)
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ILeaseDataQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<StatePointer>());

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory(sourceRepositoryMock, committedSources));

            serviceCollection.AddStreamRepository();
        });

        await using var writeRepository = await GetWriteStreamRepository(serviceScope);

        IStateKey streamKey = new Key("StreamKey");
        var expectedStreamKeyLease = streamKey.ToLease();

        IMessageKey messageKey = new Key("MessageKey");
        var expectedMessageKeyLease = messageKey.ToLease(streamKey);

        var expectedDelta = new DoNothingIdempotent(messageKey);

        // ACT

        await writeRepository.LoadOrCreate(streamKey);

        var staged = await writeRepository
            .Append(streamKey, expectedDelta);

        var committed = await writeRepository
            .Commit();

        // ASSERT

        staged.ShouldBeTrue();
        committed.ShouldBeTrue();

        committedSources.Count.ShouldBe(1);
        committedSources[0].Messages.Length.ShouldBe(1);
        committedSources[0].Messages[0].StatePointer.StateVersion.ShouldBe(StateVersion.One);
        committedSources[0].Messages[0].Delta.ShouldBe(expectedDelta);
        committedSources[0].Messages[0].AddLeases.Length.ShouldBe(2);
        committedSources[0].Messages[0].AddLeases[0].ShouldBe(expectedStreamKeyLease);
        committedSources[0].Messages[0].AddLeases[1].ShouldBe(expectedMessageKeyLease);
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenNewStream_WhenStagingNewMessageKey_ThenCommitReturnsTrue(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddStreamRepository();
        });

        await using var writeRepository = await GetWriteStreamRepository(serviceScope);

        IStateKey streamKey = new Key("StreamKey");
        var streamKeyLease = streamKey.ToLease();

        IMessageKey messageKey = new Key("MessageKey");
        var messageKeyLease = messageKey.ToLease(streamKey);

        // ACT

        await writeRepository.LoadOrCreate(streamKey);

        var staged = await writeRepository
            .Append(streamKey, new DoNothingIdempotent(messageKey));

        var committed = await writeRepository.Commit();

        var statePointerCount = await writeRepository.SourceRepository
            .EnumerateStatePointers(new MatchingLeasesDataQuery(streamKeyLease, messageKeyLease))
            .CountAsync();

        // ASSERT

        staged.ShouldBeTrue();
        committed.ShouldBeTrue();
        statePointerCount.ShouldBe(1);
    }

    [Fact]
    public async Task GivenExistingStreamMock_WhenStagingNewMessageKey_ThenCommittedSourceIsCorrect()
    {
        // ARRANGE

        var statePointer = Id.NewId() + StateVersion.One;

        var committedSources = new List<Source>();

        var sequenceMock = new MockSequence();
        var sourceRepositoryMock = new Mock<ISourceRepository>(MockBehavior.Strict);

        // First query checks if stream key lease already exists
        sourceRepositoryMock
            .InSequence(sequenceMock)
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ILeaseDataQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerablePolyfill.FromResult(new[] { statePointer }));

        // First query checks if message key lease already exists
        sourceRepositoryMock
            .InSequence(sequenceMock)
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ILeaseDataQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<StatePointer>());

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory(sourceRepositoryMock, committedSources));

            serviceCollection.AddStreamRepository();
        });

        await using var writeRepository = await GetWriteStreamRepository(serviceScope);

        IStateKey streamKey = new Key("StreamKey");
        IMessageKey messageKey = new Key("MessageKey");

        var expectedLease = messageKey.ToLease(streamKey);

        // ARRANGE ASSERTIONS

        statePointer.StateVersion.ShouldNotBe(StateVersion.Zero);

        // ACT

        await writeRepository.LoadOrCreate(streamKey);

        var staged = await writeRepository
            .Append(streamKey, new DoNothingIdempotent(messageKey));

        var committed = await writeRepository.Commit();

        // ASSERT

        staged.ShouldBeTrue();
        committed.ShouldBeTrue();

        committedSources.Count.ShouldBe(1);
        committedSources[0].Messages.Length.ShouldBe(1);
        committedSources[0].Messages[0].StatePointer.Id.ShouldBe(statePointer.Id);
        committedSources[0].Messages[0].StatePointer.StateVersion.ShouldBe(StateVersion.Zero);
        committedSources[0].Messages[0].AddLeases.Length.ShouldBe(1);
        committedSources[0].Messages[0].AddLeases[0].ShouldBe(expectedLease);
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenExistingStream_WhenStagingNewMessageKey_ThenCommitReturnsTrue(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddStreamRepository();
        });

        await using var writeRepository = await GetWriteStreamRepository(serviceScope);

        IStateKey streamKey = new Key("StreamKey");
        var streamKeyLease = streamKey.ToLease();

        IMessageKey messageKey1 = new Key("MessageKey1");
        var messageKeyLease1 = messageKey1.ToLease(streamKey);

        IMessageKey messageKey2 = new Key("MessageKey2");
        var messageKeyLease2 = messageKey2.ToLease(streamKey);

        await writeRepository.LoadOrCreate(streamKey);

        var firstStaged = await writeRepository
            .Append(streamKey, new DoNothingIdempotent(messageKey1));

        var firstCommitted = await writeRepository.Commit();

        // ARRANGE ASSERTIONS

        firstStaged.ShouldBeTrue();
        firstCommitted.ShouldBeTrue();

        // ACT

        var secondStaged = await writeRepository
            .Append(streamKey, new DoNothingIdempotent(messageKey2));

        var secondCommitted = await writeRepository.Commit();

        var statePointerCount = await writeRepository.SourceRepository
            .EnumerateStatePointers(new MatchingLeasesDataQuery(streamKeyLease, messageKeyLease1, messageKeyLease2))
            .CountAsync();

        // ASSERT

        secondStaged.ShouldBeTrue();
        secondCommitted.ShouldBeTrue();
        statePointerCount.ShouldBe(2);
    }

    [Fact]
    public async Task GivenExistingStreamMock_WhenStagingDuplicateMessageKey_ThenStageReturnsFalse()
    {
        // ARRANGE

        var statePointer = Id.NewId() + StateVersion.One;

        var sequenceMock = new MockSequence();
        var sourceRepositoryMock = new Mock<ISourceRepository>(MockBehavior.Strict);

        // First query checks if stream key lease already exists
        sourceRepositoryMock
            .InSequence(sequenceMock)
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ILeaseDataQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerablePolyfill.FromResult(new[] { statePointer }));

        // Second query checks if message key lease already exists
        sourceRepositoryMock
            .InSequence(sequenceMock)
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ILeaseDataQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerablePolyfill.FromResult(new[] { statePointer }));

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory(sourceRepositoryMock));

            serviceCollection.AddStreamRepository();
        });

        await using var writeRepository = await GetWriteStreamRepository(serviceScope);

        IStateKey streamKey = new Key("StreamKey");
        IMessageKey messageKey = new Key("MessageKey");

        await writeRepository.LoadOrCreate(streamKey);

        // ACT

        var staged = await writeRepository
            .Append(streamKey, new DoNothingIdempotent(messageKey));

        // ASSERT

        staged.ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(With_Source))]
    public async Task GivenExistingStream_WhenStagingDuplicateMessageKey_ThenStagedReturnsFalse(
        SourceRepositoryAdder sourceRepositoryAdder)
    {
        // ARRANGE

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourceRepositoryAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddStreamRepository();
        });

        await using var writeRepository = await GetWriteStreamRepository(serviceScope);

        IStateKey streamKey = new Key("StreamKey");
        IMessageKey messageKey = new Key("MessageKey");

        await writeRepository.LoadOrCreate(streamKey);

        var stagedOnce = await writeRepository
            .Append(streamKey, new DoNothingIdempotent(messageKey));

        var committedOnce = await writeRepository.Commit();

        // ARRANGE ASSERTIONS

        stagedOnce.ShouldBeTrue();
        committedOnce.ShouldBeTrue();

        // ACT

        var stagedTwice = await writeRepository
            .Append(streamKey, new DoNothingIdempotent(messageKey));

        // ASSERT

        stagedTwice.ShouldBeFalse();
    }
}
