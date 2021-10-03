using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Queries;
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
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) => await SourceDocument.GetTransactionIds(clientSessionHandle, mongoDatabase, sourceQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetTransactionIds(ICommandQuery commandQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) => await CommandDocument.GetTransactionIds(clientSessionHandle, mongoDatabase, commandQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetTransactionIds(IFactQuery factQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) => await FactDocument.GetTransactionIds(clientSessionHandle, mongoDatabase, factQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetTransactionIds(ILeaseQuery leaseQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) => await LeaseDocument.GetTransactionIds(clientSessionHandle, mongoDatabase, leaseQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetTransactionIds(ITagQuery tagQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) => await TagDocument.GetTransactionIds(clientSessionHandle, mongoDatabase, tagQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetEntityIds(ISourceQuery sourceQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) => await SourceDocument.GetEntityIds(clientSessionHandle, mongoDatabase, sourceQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetEntityIds(ICommandQuery commandQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) => await CommandDocument.GetEntityIds(clientSessionHandle, mongoDatabase, commandQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetEntityIds(IFactQuery factQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) => await FactDocument.GetEntityIds(clientSessionHandle, mongoDatabase, factQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetEntityIds(ILeaseQuery leaseQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) => await LeaseDocument.GetEntityIds(clientSessionHandle, mongoDatabase, leaseQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<Guid[]> GetEntityIds(ITagQuery tagQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) => await TagDocument.GetEntityIds(clientSessionHandle, mongoDatabase, tagQuery),
                Array.Empty<Guid>()
            );
        }

        public Task<object[]> GetSources(ISourceQuery sourceQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) =>
                {
                    var sourceDocuments = await SourceDocument.GetMany(clientSessionHandle, mongoDatabase, sourceQuery);

                    return sourceDocuments
                        .Select(sourceDocument => sourceDocument.Data.Reconstruct<object>(logger, resolvingStrategyChain))
                        .ToArray();
                },
                Array.Empty<object>()
            );
        }

        public Task<ICommand<TEntity>[]> GetCommands(ICommandQuery commandQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) =>
                {
                    var commandDocuments = await CommandDocument.GetMany(clientSessionHandle, mongoDatabase, commandQuery);

                    return commandDocuments
                        .Select(commandDocument => commandDocument.Data.Reconstruct<ICommand<TEntity>>(logger, resolvingStrategyChain))
                        .ToArray();
                },
                Array.Empty<ICommand<TEntity>>()
            );
        }

        public Task<IFact<TEntity>[]> GetFacts(IFactQuery factQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) =>
                {
                    var factDocuments = await FactDocument.GetMany(clientSessionHandle, mongoDatabase, factQuery);

                    return factDocuments
                        .Select(factDocument => factDocument.Data.Reconstruct<IFact<TEntity>>(logger, resolvingStrategyChain))
                        .ToArray();
                },
                Array.Empty<IFact<TEntity>>()
            );
        }

        public Task<ILease[]> GetLeases(ILeaseQuery leaseQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) =>
                {
                    var leaseDocuments = await LeaseDocument.GetMany(clientSessionHandle, mongoDatabase, leaseQuery);

                    return leaseDocuments
                        .Select(leaseDocument => leaseDocument.Data.Reconstruct<ILease>(logger, resolvingStrategyChain))
                        .ToArray();
                },
                Array.Empty<ILease>()
            );
        }

        public Task<ITag[]> GetTags(ITagQuery tagQuery)
        {
            return _mongoDbSession.ExecuteQuery
            (
                async (logger, resolvingStrategyChain, clientSessionHandle, mongoDatabase) =>
                {
                    var tagDocuments = await TagDocument.GetMany(clientSessionHandle, mongoDatabase, tagQuery);

                    return tagDocuments
                        .Select(tagDocument => tagDocument.Data.Reconstruct<ITag>(logger, resolvingStrategyChain))
                        .ToArray();
                },
                Array.Empty<ITag>()
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

                        if (transactionCommand.DeleteLeases.Length > 0)
                        {
                            var deleteLeasesQuery = new DeleteLeasesQuery(transactionCommand.EntityId, transactionCommand.DeleteLeases);

                            await LeaseDocument.DeleteMany
                            (
                                clientSessionHandle,
                                mongoDatabase,
                                deleteLeasesQuery
                            );
                        }

                        await LeaseDocument.InsertMany
                        (
                            clientSessionHandle,
                            mongoDatabase,
                            GetInsertLeaseDocuments(logger, transaction, transactionCommand)
                        );

                        if (transactionCommand.DeleteTags.Length > 0)
                        {
                            var deleteTagsQuery = new DeleteTagsQuery(transactionCommand.EntityId, transactionCommand.DeleteTags);

                            await TagDocument.DeleteMany
                            (
                                clientSessionHandle,
                                mongoDatabase,
                                deleteTagsQuery
                            );
                        }

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
