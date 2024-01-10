using EntityDb.Abstractions.EventStreams;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.EventStreams;
using EntityDb.Common.Extensions;
using EntityDb.Common.Polyfills;
using EntityDb.Common.Sources.Queries.Standard;
using EntityDb.Common.Tests.Implementations.Deltas;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.Common.Tests.EventStreams;

[Collection(nameof(DatabaseContainerCollection))]
public class EventStreamTests : TestsBase<Startup>
{
    public EventStreamTests(IServiceProvider serviceProvider, DatabaseContainerFixture databaseContainerFixture)
        : base(serviceProvider, databaseContainerFixture)
    {
    }

    [Fact]
    public async Task GivenNewEventStreamMock_WhenStagingNewEventKey_ThenCommittedSourceIsCorrect()
    {
        // ARRANGE

        var streamKey = new Key("StreamKey");
        var expectedStreamKeyLease = EventStreamRepository.GetStreamKeyLease(streamKey);

        var eventKey = new Key("EventKey");
        var expectedEventKeyLease = EventStreamRepository.GetEventKeyLease(streamKey, eventKey);

        var expectedDelta = new DoNothing();

        var committedSources = new List<Source>();

        var sequenceMock = new MockSequence();
        var sourceRepositoryMock = new Mock<ISourceRepository>(MockBehavior.Strict);

        // First query checks if eventKey lease already exists
        sourceRepositoryMock
            .InSequence(sequenceMock)
            .Setup(repository =>
                repository.EnumerateEntityPointers(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<Pointer>());

        // Second query checks if streamKey lease already exists
        sourceRepositoryMock
            .InSequence(sequenceMock)
            .Setup(repository =>
                repository.EnumerateEntityPointers(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<Pointer>());

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory(sourceRepositoryMock, committedSources));

            serviceCollection.AddEventStream();
        });

        await using var eventStreamRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEventStreamRepositoryFactory>()
            .CreateRepository(default!, default!);

        // ACT

        var staged = await eventStreamRepository
            .Stage(streamKey, eventKey, expectedDelta);

        var committed = await eventStreamRepository
            .Commit(Id.NewId());

        // ASSERT

        staged.ShouldBeTrue();
        committed.ShouldBeTrue();

        committedSources.Count.ShouldBe(1);
        committedSources[0].Messages.Length.ShouldBe(1);
        committedSources[0].Messages[0].EntityPointer.Version.ShouldBe(Version.Zero);
        committedSources[0].Messages[0].Delta.ShouldBe(expectedDelta);
        committedSources[0].Messages[0].AddLeases.Length.ShouldBe(2);
        committedSources[0].Messages[0].AddLeases[0].ShouldBe(expectedEventKeyLease);
        committedSources[0].Messages[0].AddLeases[1].ShouldBe(expectedStreamKeyLease);
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenNewEventStream_WhenStagingNewEventKey_ThenCommitReturnsTrue(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        var streamKey = new Key("StreamKey");
        var streamKeyLease = EventStreamRepository.GetStreamKeyLease(streamKey);

        var eventKey = new Key("EventKey");
        var eventKeyLease = EventStreamRepository.GetEventKeyLease(streamKey, eventKey);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddEventStream();
        });

        await using var eventStreamRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEventStreamRepositoryFactory>()
            .CreateRepository(default!, default!);

        // ACT

        var staged = await eventStreamRepository
            .Stage(streamKey, eventKey, new DoNothing());

        var committed = await eventStreamRepository
            .Commit(Id.NewId());

        var entityPointerCount = await eventStreamRepository.SourceRepository
            .EnumerateEntityPointers(new MatchingLeasesQuery(streamKeyLease, eventKeyLease))
            .CountAsync();

        // ASSERT

        staged.ShouldBeTrue();
        committed.ShouldBeTrue();
        entityPointerCount.ShouldBe(1);
    }

    [Fact]
    public async Task GivenExistingEventStreamMock_WhenStagingNewEventKey_ThenCommittedSourceIsCorrect()
    {
        // ARRANGE

        var entityPointer = Id.NewId() + Version.Zero.Next();
        var streamKey = new Key("StreamKey");
        var eventKey = new Key("EventKey");
        var expectedLease = EventStreamRepository.GetEventKeyLease(streamKey, eventKey);

        var committedSources = new List<Source>();

        var sequenceMock = new MockSequence();
        var sourceRepositoryMock = new Mock<ISourceRepository>(MockBehavior.Strict);

        // First query checks if eventKey lease already exists
        sourceRepositoryMock
            .InSequence(sequenceMock)
            .Setup(repository =>
                repository.EnumerateEntityPointers(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<Pointer>());

        // Second query checks if streamKey lease already exists
        sourceRepositoryMock
            .InSequence(sequenceMock)
            .Setup(repository =>
                repository.EnumerateEntityPointers(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerablePolyfill.FromResult(new[] { entityPointer }));

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory(sourceRepositoryMock, committedSources));

            serviceCollection.AddEventStream();
        });

        await using var eventStreamRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEventStreamRepositoryFactory>()
            .CreateRepository(default!, default!);

        // ARRANGE ASSERTIONS

        entityPointer.Version.ShouldNotBe(Version.Zero);

        // ACT

        var staged = await eventStreamRepository
            .Stage(streamKey, eventKey, new DoNothing());

        var committed = await eventStreamRepository
            .Commit(Id.NewId());

        // ASSERT

        staged.ShouldBeTrue();
        committed.ShouldBeTrue();

        committedSources.Count.ShouldBe(1);
        committedSources[0].Messages.Length.ShouldBe(1);
        committedSources[0].Messages[0].EntityPointer.Id.ShouldBe(entityPointer.Id);
        committedSources[0].Messages[0].EntityPointer.Version.ShouldBe(Version.Zero);
        committedSources[0].Messages[0].AddLeases.Length.ShouldBe(1);
        committedSources[0].Messages[0].AddLeases[0].ShouldBe(expectedLease);
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenExistingEventStream_WhenStagingNewEventKey_ThenCommitReturnsTrue(SourcesAdder sourcesAdder)
    {
        // ARRANGE

        var streamKey = new Key("StreamKey");
        var streamKeyLease = EventStreamRepository.GetStreamKeyLease(streamKey);

        var eventKey1 = new Key("EventKey1");
        var eventKeyLease1 = EventStreamRepository.GetEventKeyLease(streamKey, eventKey1);

        var eventKey2 = new Key("EventKey2");
        var eventKeyLease2 = EventStreamRepository.GetEventKeyLease(streamKey, eventKey2);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddEventStream();
        });

        await using var eventStreamRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEventStreamRepositoryFactory>()
            .CreateRepository(default!, default!);

        var firstStaged = await eventStreamRepository
            .Stage(streamKey, eventKey1, new DoNothing());

        var firstCommitted = await eventStreamRepository
            .Commit(Id.NewId());

        // ARRANGE ASSERTIONS

        firstStaged.ShouldBeTrue();
        firstCommitted.ShouldBeTrue();

        // ACT

        var secondStaged = await eventStreamRepository
            .Stage(streamKey, eventKey2, new DoNothing());

        var secondCommitted = await eventStreamRepository
            .Commit(Id.NewId());

        var entityPointerCount = await eventStreamRepository.SourceRepository
            .EnumerateEntityPointers(new MatchingLeasesQuery(streamKeyLease, eventKeyLease1, eventKeyLease2))
            .CountAsync();

        // ASSERT

        secondStaged.ShouldBeTrue();
        secondCommitted.ShouldBeTrue();
        entityPointerCount.ShouldBe(2);
    }

    [Fact]
    public async Task GivenExistingEventStreamMock_WhenStagingDuplicateEventKey_ThenStageReturnsFalse()
    {
        // ARRANGE

        var entityPointer = Id.NewId() + Version.Zero.Next();
        var streamKey = new Key("StreamKey");
        var eventKey = new Key("EventKey");

        var sequenceMock = new MockSequence();
        var sourceRepositoryMock = new Mock<ISourceRepository>(MockBehavior.Strict);

        // First query checks if eventKey lease already exists
        sourceRepositoryMock
            .InSequence(sequenceMock)
            .Setup(repository =>
                repository.EnumerateEntityPointers(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerablePolyfill.FromResult(new[] { entityPointer }));

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.AddSingleton(GetMockedSourceRepositoryFactory(sourceRepositoryMock));

            serviceCollection.AddEventStream();
        });

        await using var eventStreamRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEventStreamRepositoryFactory>()
            .CreateRepository(default!, default!);

        // ACT

        var staged = await eventStreamRepository
            .Stage(streamKey, eventKey, new DoNothing());

        // ASSERT

        staged.ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(AddSource))]
    public async Task GivenExistingEventStream_WhenStagingDuplicateEventKey_ThenStagedReturnsFalse(
        SourcesAdder sourcesAdder)
    {
        // ARRANGE

        var streamKey = new Key("StreamKey");
        var eventKey = new Key("EventKey");

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            sourcesAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.AddEventStream();
        });

        await using var eventStreamRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEventStreamRepositoryFactory>()
            .CreateRepository(default!, default!);

        var stagedOnce = await eventStreamRepository
            .Stage(streamKey, eventKey, new DoNothing());

        var committedOnce = await eventStreamRepository
            .Commit(Id.NewId());

        // ARRANGE ASSERTIONS

        stagedOnce.ShouldBeTrue();
        committedOnce.ShouldBeTrue();

        // ACT

        var stagedTwice = await eventStreamRepository
            .Stage(streamKey, eventKey, new DoNothing());

        // ASSERT

        stagedTwice.ShouldBeFalse();
    }
}