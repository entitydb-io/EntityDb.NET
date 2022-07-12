using System;
using System.Threading;
using System.Threading.Tasks;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using System.Linq;

namespace EntityDb.Common.Tests.Transactions;

public class TryCatchTransactionRepositoryTests : TestsBase<Startup>
{
    public TryCatchTransactionRepositoryTests(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [Fact]
    public async Task GivenRepositoryAlwaysThrows_WhenExecutingAnyMethod_ThenExceptionIsLogged()
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<Exception>();

        var transactionRepositoryMock = new Mock<ITransactionRepository>(MockBehavior.Strict);

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateTransactionIds(It.IsAny<IAgentSignatureQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateTransactionIds(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateTransactionIds(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateTransactionIds(It.IsAny<ITagQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateEntityIds(It.IsAny<IAgentSignatureQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateEntityIds(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateEntityIds(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateEntityIds(It.IsAny<ITagQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateAgentSignatures(It.IsAny<IAgentSignatureQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateCommands(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateLeases(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.EnumerateTags(It.IsAny<ITagQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository =>
                repository.EnumerateAnnotatedAgentSignatures(It.IsAny<IAgentSignatureQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository =>
                repository.EnumerateAnnotatedCommands(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.PutTransaction(It.IsAny<ITransaction>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        var tryCatchTransactionRepository = new TryCatchTransactionRepository(transactionRepositoryMock.Object,
            loggerFactory.CreateLogger<TryCatchTransactionRepository>());

        // ACT

        var transactionIdsFromAgentSignatureQuery =
            await tryCatchTransactionRepository.EnumerateTransactionIds(default(IAgentSignatureQuery)!).ToArrayAsync();
        var transactionIdsFromCommandQuery =
            await tryCatchTransactionRepository.EnumerateTransactionIds(default(ICommandQuery)!).ToArrayAsync();
        var transactionIdsFromLeaseQuery =
            await tryCatchTransactionRepository.EnumerateTransactionIds(default(ILeaseQuery)!).ToArrayAsync();
        var transactionIdsFromTagQuery =
            await tryCatchTransactionRepository.EnumerateTransactionIds(default(ITagQuery)!).ToArrayAsync();
        var entityIdsFromAgentSignatureQuery =
            await tryCatchTransactionRepository.EnumerateEntityIds(default(IAgentSignatureQuery)!).ToArrayAsync();
        var entityIdsFromCommandQuery =
            await tryCatchTransactionRepository.EnumerateEntityIds(default(ICommandQuery)!).ToArrayAsync();
        var entityIdsFromLeaseQuery =
            await tryCatchTransactionRepository.EnumerateEntityIds(default(ILeaseQuery)!).ToArrayAsync();
        var entityIdsFromTagQuery =
            await tryCatchTransactionRepository.EnumerateEntityIds(default(ITagQuery)!).ToArrayAsync();
        var agentSignatures =
            await tryCatchTransactionRepository.EnumerateAgentSignatures(default!).ToArrayAsync();
        var commands =
            await tryCatchTransactionRepository.EnumerateCommands(default!).ToArrayAsync();
        var leases =
            await tryCatchTransactionRepository.EnumerateLeases(default!).ToArrayAsync();
        var tags =
            await tryCatchTransactionRepository.EnumerateTags(default!).ToArrayAsync();
        var annotatedCommands =
            await tryCatchTransactionRepository.EnumerateAnnotatedCommands(default!).ToArrayAsync();
        var inserted =
            await tryCatchTransactionRepository.PutTransaction(default!);

        // ASSERT

        transactionIdsFromAgentSignatureQuery.ShouldBeEmpty();
        transactionIdsFromCommandQuery.ShouldBeEmpty();
        transactionIdsFromLeaseQuery.ShouldBeEmpty();
        transactionIdsFromTagQuery.ShouldBeEmpty();
        entityIdsFromAgentSignatureQuery.ShouldBeEmpty();
        entityIdsFromCommandQuery.ShouldBeEmpty();
        entityIdsFromLeaseQuery.ShouldBeEmpty();
        entityIdsFromTagQuery.ShouldBeEmpty();
        agentSignatures.ShouldBeEmpty();
        commands.ShouldBeEmpty();
        leases.ShouldBeEmpty();
        tags.ShouldBeEmpty();
        annotatedCommands.ShouldBeEmpty();
        inserted.ShouldBeFalse();
        loggerVerifier.Invoke(Times.Exactly(14));
    }
}