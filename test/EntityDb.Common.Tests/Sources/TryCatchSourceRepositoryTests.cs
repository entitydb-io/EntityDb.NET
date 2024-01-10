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

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<Exception>();

        var sourceRepositoryMock = new Mock<ISourceRepository>(MockBehavior.Strict);

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateSourceIds(It.IsAny<IMessageGroupQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateSourceIds(It.IsAny<IMessageQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateSourceIds(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateSourceIds(It.IsAny<ITagQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateEntityPointers(It.IsAny<IMessageGroupQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateEntityPointers(It.IsAny<IMessageQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateEntityPointers(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateEntityPointers(It.IsAny<ITagQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateAgentSignatures(It.IsAny<IMessageGroupQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateDeltas(It.IsAny<IMessageQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateLeases(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository => repository.EnumerateTags(It.IsAny<ITagQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateAnnotatedAgentSignatures(It.IsAny<IMessageGroupQuery>(),
                    It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.EnumerateAnnotatedDeltas(It.IsAny<IMessageQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        sourceRepositoryMock
            .Setup(repository =>
                repository.Commit(It.IsAny<Source>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        var tryCatchSourceRepository = new TryCatchSourceRepository(sourceRepositoryMock.Object,
            loggerFactory.CreateLogger<TryCatchSourceRepository>());

        // ACT

        var sourceIdsFromMessageGroupQuery =
            await tryCatchSourceRepository.EnumerateSourceIds(default(IMessageGroupQuery)!).ToArrayAsync();
        var sourceIdsFromMessageQuery =
            await tryCatchSourceRepository.EnumerateSourceIds(default(IMessageQuery)!).ToArrayAsync();
        var sourceIdsFromLeaseQuery =
            await tryCatchSourceRepository.EnumerateSourceIds(default(ILeaseQuery)!).ToArrayAsync();
        var sourceIdsFromTagQuery =
            await tryCatchSourceRepository.EnumerateSourceIds(default(ITagQuery)!).ToArrayAsync();
        var entityPointersFromMessageGroupQuery =
            await tryCatchSourceRepository.EnumerateEntityPointers(default(IMessageGroupQuery)!).ToArrayAsync();
        var entityPointersFromMessageQuery =
            await tryCatchSourceRepository.EnumerateEntityPointers(default(IMessageQuery)!).ToArrayAsync();
        var entityPointersFromLeaseQuery =
            await tryCatchSourceRepository.EnumerateEntityPointers(default(ILeaseQuery)!).ToArrayAsync();
        var entityPointersFromTagQuery =
            await tryCatchSourceRepository.EnumerateEntityPointers(default(ITagQuery)!).ToArrayAsync();
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

        sourceIdsFromMessageGroupQuery.ShouldBeEmpty();
        sourceIdsFromMessageQuery.ShouldBeEmpty();
        sourceIdsFromLeaseQuery.ShouldBeEmpty();
        sourceIdsFromTagQuery.ShouldBeEmpty();
        entityPointersFromMessageGroupQuery.ShouldBeEmpty();
        entityPointersFromMessageQuery.ShouldBeEmpty();
        entityPointersFromLeaseQuery.ShouldBeEmpty();
        entityPointersFromTagQuery.ShouldBeEmpty();
        agentSignatures.ShouldBeEmpty();
        deltas.ShouldBeEmpty();
        leases.ShouldBeEmpty();
        tags.ShouldBeEmpty();
        annotatedDeltas.ShouldBeEmpty();
        committed.ShouldBeFalse();
        loggerVerifier.Invoke(Times.Exactly(14));
    }
}