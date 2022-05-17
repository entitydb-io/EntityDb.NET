using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Transactions;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

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
            .Setup(repository => repository.GetTransactionIds(It.IsAny<IAgentSignatureQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetTransactionIds(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetTransactionIds(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetTransactionIds(It.IsAny<ITagQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetEntityIds(It.IsAny<IAgentSignatureQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetEntityIds(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetEntityIds(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetEntityIds(It.IsAny<ITagQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetAgentSignatures(It.IsAny<IAgentSignatureQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetCommands(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetLeases(It.IsAny<ILeaseQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetTags(It.IsAny<ITagQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetAnnotatedAgentSignatures(It.IsAny<IAgentSignatureQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.GetAnnotatedCommands(It.IsAny<ICommandQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        transactionRepositoryMock
            .Setup(repository => repository.PutTransaction(It.IsAny<ITransaction>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException());

        var tryCatchTransactionRepository = new TryCatchTransactionRepository(transactionRepositoryMock.Object, loggerFactory.CreateLogger<TryCatchTransactionRepository>());

        // ACT

        var transactionIdsFromAgentSignatureQuery = await tryCatchTransactionRepository.GetTransactionIds(default(IAgentSignatureQuery)!);
        var transactionIdsFromCommandQuery = await tryCatchTransactionRepository.GetTransactionIds(default(ICommandQuery)!);
        var transactionIdsFromLeaseQuery = await tryCatchTransactionRepository.GetTransactionIds(default(ILeaseQuery)!);
        var transactionIdsFromTagQuery = await tryCatchTransactionRepository.GetTransactionIds(default(ITagQuery)!);
        var entityIdsFromAgentSignatureQuery = await tryCatchTransactionRepository.GetEntityIds(default(IAgentSignatureQuery)!);
        var entityIdsFromCommandQuery = await tryCatchTransactionRepository.GetEntityIds(default(ICommandQuery)!);
        var entityIdsFromLeaseQuery = await tryCatchTransactionRepository.GetEntityIds(default(ILeaseQuery)!);
        var entityIdsFromTagQuery = await tryCatchTransactionRepository.GetEntityIds(default(ITagQuery)!);
        var agentSignatures = await tryCatchTransactionRepository.GetAgentSignatures(default!);
        var commands = await tryCatchTransactionRepository.GetCommands(default!);
        var leases = await tryCatchTransactionRepository.GetLeases(default!);
        var tags = await tryCatchTransactionRepository.GetTags(default!);
        var annotatedCommands = await tryCatchTransactionRepository.GetAnnotatedCommands(default!);
        var inserted = await tryCatchTransactionRepository.PutTransaction(default!);

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