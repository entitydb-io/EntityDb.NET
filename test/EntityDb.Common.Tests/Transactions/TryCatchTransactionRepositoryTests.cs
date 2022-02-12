using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Transactions;
using Moq;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Common.Tests.Transactions
{
    public class TryCatchTransactionRepositoryTests : TestsBase<Startup>
    {
        public TryCatchTransactionRepositoryTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [Fact]
        public async Task GivenRepositoryAlwaysThrows_WhenExecutingAnyMethod_ThenExceptionIsLogged()
        {
            // ARRANGE

            var loggerMock = new Mock<ILogger>(MockBehavior.Strict);

            loggerMock
                .Setup(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()))
                .Verifiable();

            var transactionRepositoryMock = new Mock<ITransactionRepository<TransactionEntity>>(MockBehavior.Strict);

            transactionRepositoryMock
                .Setup(repository => repository.GetTransactionIds(It.IsAny<IAgentSignatureQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.GetTransactionIds(It.IsAny<ICommandQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.GetTransactionIds(It.IsAny<ILeaseQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.GetTransactionIds(It.IsAny<ITagQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.GetEntityIds(It.IsAny<IAgentSignatureQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.GetEntityIds(It.IsAny<ICommandQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.GetEntityIds(It.IsAny<ILeaseQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.GetEntityIds(It.IsAny<ITagQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.GetAgentSignatures(It.IsAny<IAgentSignatureQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.GetCommands(It.IsAny<ICommandQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.GetLeases(It.IsAny<ILeaseQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.GetTags(It.IsAny<ITagQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.GetAnnotatedCommands(It.IsAny<ICommandQuery>()))
                .ThrowsAsync(new NotImplementedException());

            transactionRepositoryMock
                .Setup(repository => repository.PutTransaction(It.IsAny<ITransaction<TransactionEntity>>()))
                .ThrowsAsync(new NotImplementedException());

            var tryCatchTransactionRepository = new TryCatchTransactionRepository<TransactionEntity>(transactionRepositoryMock.Object, loggerMock.Object);

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

            loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Exactly(14));
        }
    }
}
