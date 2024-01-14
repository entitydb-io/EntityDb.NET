using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Extensions;
using EntityDb.Common.Polyfills;
using EntityDb.Common.Sources.Queries.Standard;
using EntityDb.Common.Streams;
using EntityDb.Common.Tests.Implementations.Entities.Deltas;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;
using Version = EntityDb.Abstractions.ValueObjects.Version;

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
            .Returns(AsyncEnumerable.Empty<Pointer>());

        // Second query checks if message key lease already exists
        sourceRepositoryMock
            .InSequence(sequenceMock)
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ILeaseDataQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<Pointer>());

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory(sourceRepositoryMock, committedSources));

            serviceCollection.AddStreamRepository();
        });

        await using var writeRepository = await GetWriteStreamRepository(serviceScope);

        var streamKey = new Key("StreamKey");
        var expectedStreamKeyLease = MultipleStreamRepository.GetStreamKeyLease(streamKey);

        var messageKey = new Key("MessageKey");
        var expectedMessageKeyLease = MultipleStreamRepository.GetMessageKeyLease(streamKey, messageKey);

        var expectedDelta = new DoNothing();

        // ACT

        await writeRepository.LoadOrCreate(streamKey);

        var staged = await writeRepository
            .Append(streamKey, messageKey, expectedDelta);

        var committed = await writeRepository
            .Commit();

        // ASSERT

        staged.ShouldBeTrue();
        committed.ShouldBeTrue();

        committedSources.Count.ShouldBe(1);
        committedSources[0].Messages.Length.ShouldBe(1);
        committedSources[0].Messages[0].StatePointer.Version.ShouldBe(Version.Zero);
        committedSources[0].Messages[0].Delta.ShouldBe(expectedDelta);
        committedSources[0].Messages[0].AddLeases.Length.ShouldBe(2);
        committedSources[0].Messages[0].AddLeases[0].ShouldBe(expectedMessageKeyLease);
        committedSources[0].Messages[0].AddLeases[1].ShouldBe(expectedStreamKeyLease);
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

        var streamKey = new Key("StreamKey");
        var streamKeyLease = MultipleStreamRepository.GetStreamKeyLease(streamKey);

        var messageKey = new Key("MessageKey");
        var messageKeyLease = MultipleStreamRepository.GetMessageKeyLease(streamKey, messageKey);

        // ACT

        await writeRepository.LoadOrCreate(streamKey);

        var staged = await writeRepository
            .Append(streamKey, messageKey, new DoNothing());

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

        var statePointer = Id.NewId() + Version.Zero.Next();

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
            .Returns(AsyncEnumerable.Empty<Pointer>());

        await using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory(sourceRepositoryMock, committedSources));

            serviceCollection.AddStreamRepository();
        });

        await using var writeRepository = await GetWriteStreamRepository(serviceScope);

        var streamKey = new Key("StreamKey");
        var messageKey = new Key("MessageKey");

        var expectedLease = MultipleStreamRepository.GetMessageKeyLease(streamKey, messageKey);

        // ARRANGE ASSERTIONS

        statePointer.Version.ShouldNotBe(Version.Zero);

        // ACT

        await writeRepository.LoadOrCreate(streamKey);

        var staged = await writeRepository
            .Append(streamKey, messageKey, new DoNothing());

        var committed = await writeRepository.Commit();

        // ASSERT

        staged.ShouldBeTrue();
        committed.ShouldBeTrue();

        committedSources.Count.ShouldBe(1);
        committedSources[0].Messages.Length.ShouldBe(1);
        committedSources[0].Messages[0].StatePointer.Id.ShouldBe(statePointer.Id);
        committedSources[0].Messages[0].StatePointer.Version.ShouldBe(Version.Zero);
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

        var streamKey = new Key("StreamKey");
        var streamKeyLease = MultipleStreamRepository.GetStreamKeyLease(streamKey);

        var messageKey1 = new Key("MessageKey1");
        var messageKeyLease1 = MultipleStreamRepository.GetMessageKeyLease(streamKey, messageKey1);

        var messageKey2 = new Key("MessageKey2");
        var messageKeyLease2 = MultipleStreamRepository.GetMessageKeyLease(streamKey, messageKey2);

        await writeRepository.LoadOrCreate(streamKey);

        var firstStaged = await writeRepository
            .Append(streamKey, messageKey1, new DoNothing());

        var firstCommitted = await writeRepository.Commit();

        // ARRANGE ASSERTIONS

        firstStaged.ShouldBeTrue();
        firstCommitted.ShouldBeTrue();

        // ACT

        var secondStaged = await writeRepository
            .Append(streamKey, messageKey2, new DoNothing());

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

        var statePointer = Id.NewId() + Version.Zero.Next();

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

        var streamKey = new Key("StreamKey");
        var messageKey = new Key("MessageKey");

        await writeRepository.LoadOrCreate(streamKey);

        // ACT

        var staged = await writeRepository
            .Append(streamKey, messageKey, new DoNothing());

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

        var streamKey = new Key("StreamKey");
        var messageKey = new Key("MessageKey");

        await writeRepository.LoadOrCreate(streamKey);

        var stagedOnce = await writeRepository
            .Append(streamKey, messageKey, new DoNothing());

        var committedOnce = await writeRepository.Commit();

        // ARRANGE ASSERTIONS

        stagedOnce.ShouldBeTrue();
        committedOnce.ShouldBeTrue();

        // ACT

        var stagedTwice = await writeRepository
            .Append(streamKey, messageKey, new DoNothing());

        // ASSERT

        stagedTwice.ShouldBeFalse();
    }
}
