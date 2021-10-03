using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Sessions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions
{
    internal sealed class MongoDbTransactionRepository<TEntity> : ITransactionRepository<TEntity>
    {
        private readonly IMongoDbSession _mongoDbSession;

        public MongoDbTransactionRepository(IMongoDbSession mongoDbSession)
        {
            _mongoDbSession = mongoDbSession;
        }

        public Task<Guid[]> GetTransactionIds(ISourceQuery sourceQuery)
        {
            return _mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => SourceDocument.GetTransactionIdsQuery(clientSessionHandle, mongoDatabase, sourceQuery)
            );
        }

        public Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery)
        {
            return _mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => CommandDocument.GetTransactionIdsQuery(clientSessionHandle, mongoDatabase, commandQuery)
            );
        }

        public Task<Guid[]> GetTransactionIds(IFactQuery factQuery)
        {
            return _mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => FactDocument.GetTransactionIdsQuery(clientSessionHandle, mongoDatabase, factQuery)
            );
        }

        public Task<Guid[]> GetTransactionIds(ILeaseQuery leaseQuery)
        {
            return _mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => LeaseDocument.GetTransactionIds(clientSessionHandle, mongoDatabase, leaseQuery)
            );
        }

        public Task<Guid[]> GetTransactionIds(ITagQuery tagQuery)
        {
            return _mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => TagDocument.GetTransactionIds(clientSessionHandle, mongoDatabase, tagQuery)
            );
        }

        public Task<Guid[]> GetEntityIds(ISourceQuery sourceQuery)
        {
            return _mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => SourceDocument.GetEntityIdsQuery(clientSessionHandle, mongoDatabase, sourceQuery)
            );
        }

        public Task<Guid[]> GetEntityIds(ICommandQuery commandQuery)
        {
            return _mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => CommandDocument.GetEntityIdsQuery(clientSessionHandle, mongoDatabase, commandQuery)
            );
        }

        public Task<Guid[]> GetEntityIds(IFactQuery factQuery)
        {
            return _mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => FactDocument.GetEntityIdsQuery(clientSessionHandle, mongoDatabase, factQuery)
            );
        }

        public Task<Guid[]> GetEntityIds(ILeaseQuery leaseQuery)
        {
            return _mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => LeaseDocument.GetEntityIdsQuery(clientSessionHandle, mongoDatabase, leaseQuery)
            );
        }

        public Task<Guid[]> GetEntityIds(ITagQuery tagQuery)
        {
            return _mongoDbSession.ExecuteGuidQuery
            (
                (clientSessionHandle, mongoDatabase) => TagDocument.GetEntityIdsQuery(clientSessionHandle, mongoDatabase, tagQuery)
            );
        }

        public Task<object[]> GetSources(ISourceQuery sourceQuery)
        {
            return _mongoDbSession.ExecuteDataQuery<SourceDocument, object>
            (
                (clientSessionHandle, mongoDatabase) => SourceDocument.GetDataQuery(clientSessionHandle, mongoDatabase, sourceQuery)
            );
        }

        public Task<ICommand<TEntity>[]> GetCommands(ICommandQuery commandQuery)
        {
            return _mongoDbSession.ExecuteDataQuery<CommandDocument, ICommand<TEntity>>
            (
                (clientSessionHandle, mongoDatabase) => CommandDocument.GetDataQuery(clientSessionHandle, mongoDatabase, commandQuery)
            );
        }

        public Task<IFact<TEntity>[]> GetFacts(IFactQuery factQuery)
        {
            return _mongoDbSession.ExecuteDataQuery<FactDocument, IFact<TEntity>>
            (
                (clientSessionHandle, mongoDatabase) => FactDocument.GetDataQuery(clientSessionHandle, mongoDatabase, factQuery)
            );
        }

        public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
        {
            return _mongoDbSession.ExecuteDataQuery<LeaseDocument, ILease>
            (
                (clientSessionHandle, mongoDatabase) => LeaseDocument.GetDataQuery(clientSessionHandle, mongoDatabase, leaseQuery)
            );
        }

        public Task<ITag[]> GetTags(ITagQuery tagQuery)
        {
            return _mongoDbSession.ExecuteDataQuery<TagDocument, ITag>
            (
                (clientSessionHandle, mongoDatabase) => TagDocument.GetDataQuery(clientSessionHandle, mongoDatabase, tagQuery)
            );
        }

        private static SourceDocument GetSourceDocument
        (
            ILogger logger,
            ITransaction<TEntity> transaction
        )
        {
            return new SourceDocument
            (
                transaction.TimeStamp,
                transaction.Id,
                transaction.Commands.Select(command => command.EntityId).Distinct().ToArray(),
                BsonDocumentEnvelope.Deconstruct(transaction.Source, logger)
            );
        }

        private static CommandDocument GetCommandDocument
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionCommand<TEntity> transactionCommand
        )
        {
            return new CommandDocument
            (
                transaction.TimeStamp,
                transaction.Id,
                transactionCommand.EntityId,
                transactionCommand.ExpectedPreviousVersionNumber + 1,
                BsonDocumentEnvelope.Deconstruct(transactionCommand.Command, logger)
            );
        }

        private static IEnumerable<FactDocument> GetFactDocuments
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionCommand<TEntity> transactionCommand
        )
        {
            return transactionCommand.Facts
                .Select(transactionFact => new FactDocument
                (
                    transaction.TimeStamp,
                    transaction.Id,
                    transactionCommand.EntityId,
                    transactionCommand.ExpectedPreviousVersionNumber + 1,
                    transactionFact.SubversionNumber,
                    BsonDocumentEnvelope.Deconstruct(transactionFact.Fact, logger)
                ));
        }

        private static IEnumerable<LeaseDocument> GetInsertLeaseDocuments
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionCommand<TEntity> transactionCommand
        )
        {
            return transactionCommand.InsertLeases
                .Select(insertLease => new LeaseDocument
                (
                    transaction.TimeStamp,
                    transaction.Id,
                    transactionCommand.EntityId,
                    transactionCommand.ExpectedPreviousVersionNumber + 1,
                    insertLease.Scope,
                    insertLease.Label,
                    insertLease.Value,
                    BsonDocumentEnvelope.Deconstruct(insertLease, logger)
                ));
        }

        private static IEnumerable<TagDocument> GetInsertTagDocuments
        (
            ILogger logger,
            ITransaction<TEntity> transaction,
            ITransactionCommand<TEntity> transactionCommand
        )
        {
            return transactionCommand.InsertTags
                .Select(insertTag => new TagDocument
                (
                    transaction.TimeStamp,
                    transaction.Id,
                    transactionCommand.EntityId,
                    transactionCommand.ExpectedPreviousVersionNumber + 1,
                    insertTag.Label,
                    insertTag.Value,
                    BsonDocumentEnvelope.Deconstruct(insertTag, logger)
                ));
        }

        public Task<bool> PutTransaction(ITransaction<TEntity> transaction)
        {
            return _mongoDbSession.ExecuteCommand
            (
                async (logger, clientSessionHandle, mongoDatabase) =>
                {
                    await SourceDocument.InsertOne
                    (
                        clientSessionHandle,
                        mongoDatabase,
                        GetSourceDocument(logger, transaction)
                    );

                    foreach (var transactionCommand in transaction.Commands)
                    {
                        var actualPreviousVersion = await CommandDocument.GetLastEntityVersionNumber
                        (
                            clientSessionHandle,
                            mongoDatabase,
                            transactionCommand.EntityId
                        );

                        if (transactionCommand.ExpectedPreviousVersionNumber != actualPreviousVersion)
                        {
                            throw new OptimisticConcurrencyException();
                        }

                        var nextVersionNumber = transactionCommand.ExpectedPreviousVersionNumber + 1;

                        await CommandDocument.InsertOne
                        (
                            clientSessionHandle,
                            mongoDatabase,
                            GetCommandDocument(logger, transaction, transactionCommand)
                        );

                        await FactDocument.InsertMany
                        (
                            clientSessionHandle,
                            mongoDatabase,
                            GetFactDocuments(logger, transaction, transactionCommand)
                        );

                        await LeaseDocument.DeleteMany
                        (
                            clientSessionHandle,
                            mongoDatabase,
                            transactionCommand.EntityId,
                            transactionCommand.DeleteLeases
                        );

                        await LeaseDocument.InsertMany
                        (
                            clientSessionHandle,
                            mongoDatabase,
                            GetInsertLeaseDocuments(logger, transaction, transactionCommand)
                        );

                        await TagDocument.DeleteMany
                        (
                            clientSessionHandle,
                            mongoDatabase,
                            transactionCommand.EntityId,
                            transactionCommand.DeleteTags
                        );

                        await TagDocument.InsertMany
                        (
                            clientSessionHandle,
                            mongoDatabase,
                            GetInsertTagDocuments(logger, transaction, transactionCommand)
                        );
                    }
                }
            );
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            await _mongoDbSession.DisposeAsync();
        }
    }
}
