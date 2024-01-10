using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Common.Sources;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Sources;

public class TryCatchSourceRepositoryTests : TestsBase<Startup>
{
    public TryCatchSourceRepositoryTests(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Fact]
    public async Task GivenRepositoryAlwaysThrows_WhenExecutingAnyMethod_ThenExceptionIsLogged()
    {
        // ARRANGE

        var logs = new List<Log>();

        var loggerFactory = GetMockedLoggerFactory(logs);

        var sourceRepositoryMock = new Mock<ISourceRepository>(MockBehavior.Strict);

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateSourceIds(It.IsAny<ISourceDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateSourceIds(It.IsAny<IMessageDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateSourceIds(It.IsAny<ILeaseDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateSourceIds(It.IsAny<ITagDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ISourceDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<IMessageDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ILeaseDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ITagDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateAgentSignatures(It.IsAny<ISourceDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateDeltas(It.IsAny<IMessageDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateLeases(It.IsAny<ILeaseDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateTags(It.IsAny<ITagDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateAnnotatedAgentSignatures(It.IsAny<ISourceDataDataQuery>(),
                    It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateAnnotatedDeltas(It.IsAny<IMessageDataDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.Commit(It.IsAny<Source>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        var tryCatchSourceRepository = new TryCatchSourceRepository(sourceRepositoryMock.Object,
            loggerFactory.CreateLogger<TryCatchSourceRepository>());

        // ACT

        var sourceIdsFromSourceDataQuery =
            await tryCatchSourceRepository.EnumerateSourceIds(default(ISourceDataDataQuery)!).ToArrayAsync();
        var sourceIdsFromMessageDataQuery =
            await tryCatchSourceRepository.EnumerateSourceIds(default(IMessageDataDataQuery)!).ToArrayAsync();
        var sourceIdsFromLeaseDataQuery =
            await tryCatchSourceRepository.EnumerateSourceIds(default(ILeaseDataDataQuery)!).ToArrayAsync();
        var sourceIdsFromTagDataQuery =
            await tryCatchSourceRepository.EnumerateSourceIds(default(ITagDataDataQuery)!).ToArrayAsync();
        var statePointersFromSourceDataQuery =
            await tryCatchSourceRepository.EnumerateStatePointers(default(ISourceDataDataQuery)!).ToArrayAsync();
        var statePointersFromMessageDataQuery =
            await tryCatchSourceRepository.EnumerateStatePointers(default(IMessageDataDataQuery)!).ToArrayAsync();
        var statePointersFromLeaseDataQuery =
            await tryCatchSourceRepository.EnumerateStatePointers(default(ILeaseDataDataQuery)!).ToArrayAsync();
        var statePointersFromTagDataQuery =
            await tryCatchSourceRepository.EnumerateStatePointers(default(ITagDataDataQuery)!).ToArrayAsync();
        var agentSignatures =
            await tryCatchSourceRepository.EnumerateAgentSignatures(default!).ToArrayAsync();
        var deltas =
            await tryCatchSourceRepository.EnumerateDeltas(default!).ToArrayAsync();
        var leases =
            await tryCatchSourceRepository.EnumerateLeases(default!).ToArrayAsync();
        var tags =
            await tryCatchSourceRepository.EnumerateTags(default!).ToArrayAsync();
        var annotatedDeltas =
            await tryCatchSourceRepository.EnumerateAnnotatedDeltas(default!).ToArrayAsync();
        var committed =
            await tryCatchSourceRepository.Commit(default!);

        // ASSERT

        sourceIdsFromSourceDataQuery.ShouldBeEmpty();
        sourceIdsFromMessageDataQuery.ShouldBeEmpty();
        sourceIdsFromLeaseDataQuery.ShouldBeEmpty();
        sourceIdsFromTagDataQuery.ShouldBeEmpty();
        statePointersFromSourceDataQuery.ShouldBeEmpty();
        statePointersFromMessageDataQuery.ShouldBeEmpty();
        statePointersFromLeaseDataQuery.ShouldBeEmpty();
        statePointersFromTagDataQuery.ShouldBeEmpty();
        agentSignatures.ShouldBeEmpty();
        deltas.ShouldBeEmpty();
        leases.ShouldBeEmpty();
        tags.ShouldBeEmpty();
        annotatedDeltas.ShouldBeEmpty();
        committed.ShouldBeFalse();

        logs.Count(log => log.Exception is NotImplementedException).ShouldBe(14);
    }
}
