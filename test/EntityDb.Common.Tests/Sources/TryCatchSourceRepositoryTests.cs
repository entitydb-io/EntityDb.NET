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
                repository.EnumerateSourceIds(It.IsAny<ISourceDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateSourceIds(It.IsAny<IMessageDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateSourceIds(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateSourceIds(It.IsAny<ITagQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ISourceDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<IMessageDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateStatePointers(It.IsAny<ITagQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateAgentSignatures(It.IsAny<ISourceDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateDeltas(It.IsAny<IMessageDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateLeases(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateTags(It.IsAny<ITagQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateAnnotatedAgentSignatures(It.IsAny<ISourceDataQuery>(),
                    It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateAnnotatedDeltas(It.IsAny<IMessageDataQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.Commit(It.IsAny<Source>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        var tryCatchSourceRepository = new TryCatchSourceRepository(sourceRepositoryMock.Object,
            loggerFactory.CreateLogger<TryCatchSourceRepository>());

        // ACT

        var sourceIdsFromSourceDataQuery =
            await tryCatchSourceRepository.EnumerateSourceIds(default(ISourceDataQuery)!).ToArrayAsync();
        var sourceIdsFromMessageDataQuery =
            await tryCatchSourceRepository.EnumerateSourceIds(default(IMessageDataQuery)!).ToArrayAsync();
        var sourceIdsFromLeaseQuery =
            await tryCatchSourceRepository.EnumerateSourceIds(default(ILeaseQuery)!).ToArrayAsync();
        var sourceIdsFromTagQuery =
            await tryCatchSourceRepository.EnumerateSourceIds(default(ITagQuery)!).ToArrayAsync();
        var statePointersFromSourceDataQuery =
            await tryCatchSourceRepository.EnumerateStatePointers(default(ISourceDataQuery)!).ToArrayAsync();
        var statePointersFromMessageDataQuery =
            await tryCatchSourceRepository.EnumerateStatePointers(default(IMessageDataQuery)!).ToArrayAsync();
        var statePointersFromLeaseQuery =
            await tryCatchSourceRepository.EnumerateStatePointers(default(ILeaseQuery)!).ToArrayAsync();
        var statePointersFromTagQuery =
            await tryCatchSourceRepository.EnumerateStatePointers(default(ITagQuery)!).ToArrayAsync();
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
        sourceIdsFromLeaseQuery.ShouldBeEmpty();
        sourceIdsFromTagQuery.ShouldBeEmpty();
        statePointersFromSourceDataQuery.ShouldBeEmpty();
        statePointersFromMessageDataQuery.ShouldBeEmpty();
        statePointersFromLeaseQuery.ShouldBeEmpty();
        statePointersFromTagQuery.ShouldBeEmpty();
        agentSignatures.ShouldBeEmpty();
        deltas.ShouldBeEmpty();
        leases.ShouldBeEmpty();
        tags.ShouldBeEmpty();
        annotatedDeltas.ShouldBeEmpty();
        committed.ShouldBeFalse();

        logs.Count(log => log.Exception is NotImplementedException).ShouldBe(14);
    }
}
